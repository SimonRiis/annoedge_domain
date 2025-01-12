
using domain.algobook;
using domain.book;
using domain.exchangebook;
using domain.order;
using domain.position;
using domain.product;
using domain.shared.enums;
using domain.shared.repositories;
using LanguageExt;
using LanguageExt.Common;
using Microsoft.Extensions.Logging;
using System.Runtime.InteropServices;
using static LanguageExt.Prelude;

namespace domain.services.position
{
    public class PositionManagementService(ILogger<PositionManagementService> logger, IOrderService orderService, IPositionService positionService, IBookService bookService, IFillRepository fillRepository, IUnitOfWork unitOfWork, Guid globalBookId) : IPositionManagementService
    {
        private Fin<Unit> ProcessBookAllocation(
        Book globalBook,
        Book exchangeBook,
        Book legBook,
        Product product,
        Direction direction,
        decimal volume,
        decimal price)
        {
            return ProcessBooks(globalBook, exchangeBook, legBook);

            Fin<Unit> ProcessBooks(Book globalBook, Book exchangeBook, Book legBook)
            {
                Func<Book, BookEntry, Fin<Unit>> updateAction = direction == Direction.LONG
                    ? (book, entry) => book.IncreaseBookEntryQuantity(entry.Id, volume, price)
                    : (book, entry) => book.DecreaseBookEntryQuantity(entry.Id, volume);

                var globalBookUpdateRes = UpdateBook(globalBook, "exchange", updateAction);
                var exchangeBookUpdateRes = UpdateBook(exchangeBook, "exchange", updateAction);
                var legBookUpdateRes = UpdateBook(legBook, "leg", updateAction);

                if (exchangeBookUpdateRes.IsSucc && exchangeBookUpdateRes.IsSucc && legBookUpdateRes.IsSucc)
                {
                    return FinSucc(unit);
                }

                return FinFail<Unit>("Failed to process one or more book updates.");
            }

            Fin<Unit> UpdateBook(Book book, string bookType, Func<Book, BookEntry, Fin<Unit>> updateAction)
            {
                return book.GetBookEntryByProductId(product.Id).Match(
                    Some: entry => updateAction(book, entry),
                    None: () => FinFail<Unit>($"Could not find {bookType} book entry for product: {product.Id}")
                );
            }
        }

        private async Task<Fin<Unit>> UpdateDatabase(Position position, Order order, Book globalBook, Book exchangeBook, Book legBook)
        {
            try
            {
                await unitOfWork.Begin();
                var positionRes = await positionService.UpdateAsync(position);

                if (positionRes.IsFail)
                {
                    await unitOfWork.Rollback();
                    return positionRes.UnwrapError().ToFin().Bind(error =>
                    {
                        logger.LogError(error.Message);
                        return FinFail<Unit>(error);
                    });
                }

                var orderRes = await orderService.UpdateAsync(order);

                if (orderRes.IsFail)
                {
                    await unitOfWork.Rollback();
                    return orderRes.UnwrapError().ToFin().Bind(error =>
                    {
                        logger.LogError(error.Message);
                        return FinFail<Unit>(error);
                    });
                }

                var globalBookUpdateRes = await bookService.UpdateAsync(globalBook);

                if (globalBookUpdateRes.IsFail)
                {
                    await unitOfWork.Rollback();
                    return globalBookUpdateRes.UnwrapError().ToFin().Bind(error =>
                    {
                        logger.LogError(error.Message);
                        return FinFail<Unit>(error);
                    });
                }

                var legBookUpdateRes = await bookService.UpdateAsync(legBook);

                if (legBookUpdateRes.IsFail)
                {
                    await unitOfWork.Rollback();
                    return legBookUpdateRes.UnwrapError().ToFin().Bind(error =>
                    {
                        logger.LogError(error.Message);
                        return FinFail<Unit>(error);
                    });
                }

                var exchangeBookUpdateRes = await bookService.UpdateAsync(exchangeBook);

                if (exchangeBookUpdateRes.IsFail)
                {
                    await unitOfWork.Rollback();
                    return exchangeBookUpdateRes.UnwrapError().ToFin().Bind(error =>
                    {
                        logger.LogError(error.Message);
                        return FinFail<Unit>(error);
                    });
                }

                await unitOfWork.Commit();
                return FinSucc(unit);
            }
            catch (Exception ex)
            {
                await unitOfWork.Rollback();
                logger.LogError(ex, "Failed to update position, order or books");
                return FinFail<Unit>(ex.Message);
            }
        }


        private async Task<Fin<Unit>> ProcessPosition(Position position, Order order)
        {

            var fillsForPosition = order.Fills.OrderBy(x => x.ExchangeTimestamp);
            var exchange = order.Exchange;
            var product = order.Product;
            var leg = position.Legs.FirstOrDefault(l => l.Product.Id == product.Id && l.Exchange.Id == exchange.Id); //TODO change to be method on position

            if (leg == null)
            {
                return Error.New($"Could not find leg: productId: {product.Id} and Exchange: {exchange.Id}");
            }

            var legBookRes = await bookService.GetForId(leg.BookId);
            var exchangeBookRes = await bookService.GetForId(exchange.DefaultBookId); // main aggregated exchange book
            var globalBookRes = await bookService.GetForId(globalBookId);  // global aggregated exchange book

            return await globalBookRes.Match(

                Some: async globalBook =>
                {
                    return await exchangeBookRes.Match(

                        Some: async exchangeBook =>
                        {
                            return await legBookRes.Match(

                                Some: async legBook =>
                                {
                                    var results = fillsForPosition.Select(fill =>
                                    {

                                        if (fill.Direction == leg.Direction)
                                        {
                                            //same direction as leg, meaning we are adding to the position update intents
                                            var _ = position.DecreasePositionLegIntent(leg.Id, fill.Quantity).Bind<Fin<Unit>>(qty =>
                                            {
                                                if (qty < 0)
                                                {
                                                    //When adding support for intent based qty match for positions, the excess qty returned should be matched to the next position. 
                                                    //Basically we need to loop over the fills and match them to the intents in the right order. 
                                                    //And only go on to the next fill when all the qty to match for the current fill is matched.
                                                    logger.LogError($"Excess fill quantity for position: {position.Id}, leg: {leg.Id} and fill: {fill.Id} with quantity: {qty}");
                                                }
                                                return FinSucc(Unit.Default);
                                            }
                                            );
                                        }

                                        if (fill.Direction == leg.Direction && fill.Status != FillStatus.FullyApplied) //opening or increasing position
                                        {
                                            return
                                            position.IncreaseLeg(leg.Id, fill.Quantity, fill.Price) //if long increase long if short increase short
                                              .Bind<Fin<Unit>>(_ =>
                                              {
                                                  logger.LogInformation($"PositionId: {position.Id} increased leg: {leg.Id} with {fill.Quantity} qty from fill: {fill.Id}");
                                                  return FinSucc(Unit.Default);
                                              })
                                            .Bind<Fin<Unit>>(_ => order.UpdateFillStatus(fill.Id, FillStatus.FullyApplied))
                                            .Bind<Fin<Unit>>(_ =>
                                            {
                                                logger.LogInformation($"PositionId: {position.Id} updated fill: {fill.Id} status to {FillStatus.FullyApplied}");
                                                return FinSucc(Unit.Default);
                                            })
                                            .Bind<Fin<Unit>>(_ => ProcessBookAllocation(globalBook, exchangeBook, legBook, product, order.Direction, fill.Quantity, fill.Price));  //book.GetBookEntryByProductId(product.Id), $"Unknown direction for fill: {fill.Id}")
                                        }
                                        else if (fill.Direction != leg.Direction && fill.Status != FillStatus.FullyApplied) //closing or decreasing position
                                        {
                                            return position.DecreaseLeg(leg.Id, fill.Quantity) //if the fill is in the opposite direction we need to decrease the leg to support closing long or short positions
                                             .Bind<Fin<Unit>>(_ =>
                                             {
                                                 logger.LogInformation($"PositionId: {position.Id} decreased leg: {leg.Id} with {fill.Quantity} qty from fill: {fill.Id}");
                                                 return FinSucc(Unit.Default);
                                             })
                                            .Bind<Fin<Unit>>(_ => order.UpdateFillStatus(fill.Id, FillStatus.FullyApplied))
                                                .Bind<Fin<Unit>>(_ =>
                                                {
                                                    logger.LogInformation($"PositionId: {position.Id} updated fill: {fill.Id} status to {FillStatus.FullyApplied}");
                                                    return FinSucc(Unit.Default);
                                                })
                                                .Bind<Fin<Unit>>(_ => ProcessBookAllocation(globalBook, exchangeBook, legBook, product, order.Direction, fill.Quantity, fill.Price));  //book.GetBookEntryByProductId(product.Id), $"Unknown direction for fill: {fill.Id}")
                                        }
                                        else
                                        {
                                            return FinFail<Unit>($"Unkown direction for fill: {fill.Id}"); // Return success for other cases if needed
                                        }
                                    }).ToList(); // Collect all results

                                    if (results.Fails().Any())
                                    {
                                        return FinFail<Unit>(string.Join(", ", results.Fails().Select(r => r.Message)));
                                    }
                                    var oldPositionStatus = position.Status;
                                    var positionEventType = position.Status switch
                                    {
                                        PositionStatus.PENDING => PositionEventType.OPEN,
                                        PositionStatus.OPEN => PositionEventType.OPEN,
                                        PositionStatus.CLOSING => PositionEventType.CLOSING,
                                        PositionStatus.CLOSED => PositionEventType.CLOSE,
                                        _ => PositionEventType.OPEN
                                    };
                                    return await position.UpdateStatusBasedOnIntents().Bind(_ => position.AddEvent(
                                                    positionEventType,
                                                    oldPositionStatus,
                                                    position.Status,
                                                    "UPDATED BY POSITION MANAGEMENT SERVICE"
                                                )).BindAsync(_ => UpdateDatabase(position, order, globalBook, exchangeBook, legBook));

                                },
                                None: () => Task.FromResult(FinFail<Unit>("Could not find book for leg"))
                                );
                        },
                        None: () => Task.FromResult(FinFail<Unit>("Could not find exchange book")));
                },
                None: () => Task.FromResult(FinFail<Unit>("Could not find global book")));
        }

        private async Task<Fin<Unit>> ProcessOrder(Order order)
        {
            var position = await positionService.GetPositionForOrderIdAsync(order.ExternalId); //order and position is mapped using the orders external id (external id is the id from the exchange)
            return await position.Match(
                Succ: async position => await ProcessPosition(position, order),
                Fail: error =>
                {
                    logger.LogError($"Failed to get position for order {order.Id}", error);
                    return Task.FromResult(FinFail<Unit>(error));
                }
            );
        }

        //    //TODO: When supporting limit orders we need to loop over fills and get the order for the fill
        //    //This is due to the fact that we need to allocate the books in the right order. 
        //    //However book allocation might be splitted out into a different service.
        //    //Get grouped positions for the orders
        //    //Note that a position can be for multiple orders, but one order can only have one position
        //    //That will change when intent based volume mapping is added
        //    //But that means that we now can loop over the orders, and apply all the fills for that order to the position

        public async Task<Fin<Unit>> UpdatePositions()
        {
            // Get all orders with unhandled fills
            var orders = await orderService.GetOrdersWithUnhandledFills();

            return await orders.Match(
                async orders =>
                {
                    foreach (var order in orders.OrderBy(o => o.ExchangeTimestamp))
                    {
                        var fills = await fillRepository.GetFillsForOrderId(order.Id);

                        var res = await fills.BindAsync(async fills =>
                        {
                            var res = fills.Map(fill =>
                            {
                                var addedFill = order.AddFill(fill);
                                return addedFill.Match(
                                    Succ: _ =>
                                    {
                                        logger.LogInformation($"Added fill {fill.Id} to order {order.Id}");
                                        return FinSucc(Unit.Default);
                                    },
                                    Fail: error =>
                                    {
                                        logger.LogError($"Failed to add fill {fill.Id} to order {order.Id}", error);
                                        return FinFail<Unit>(error);
                                    }
                                );
                            });

                            if (res.ToArray().FlattenResult().IsFail)
                            {
                                return FinFail<Unit>($"Failed to add fills to order {order.Id}");
                            }

                            return await ProcessOrder(order);
                        });

                        res.Match(

                            Succ: _ => logger.LogInformation($"Processed order {order.Id}"),
                            Fail: error => logger.LogError($"Failed to process order {order.Id}", error)

                            );
                        //if (res.IsFail)
                        //{
                        //    logger.LogError($"Failed to process order {order.Id}"); //TODO should we continue with the next order or stop? 
                        //} // TODO: Should we continue with the next order or stop? For now we continue with the next order
                    }

                    return FinSucc(Unit.Default);
                },
                error => Task.FromResult(FinFail<Unit>(error))
            );
        }
    }
}

