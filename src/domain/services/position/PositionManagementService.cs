
using domain.algobook;
using domain.book;
using domain.exchangebook;
using domain.order;
using domain.position;
using domain.shared;
using domain.shared.enums;
using domain.shared.repositories;
using LanguageExt;
using LanguageExt.Common;
using Microsoft.Extensions.Logging;
using static LanguageExt.Prelude;

namespace domain.services.position
{
    public class PositionManagementService(ILogger logger, IOrderService orderService, IPositionService positionService, IBookService bookService, IUnitOfWork unitOfWork, Guid globalBookId) : IPositionManagementService
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

                                        if(fill.Direction == leg.Direction)
                                        {
                                            //same direction as leg, meaning we are adding to the position update intents
                                            var _ = position.DecreasePositionLegIntent(leg, fill.Quantity).Bind<Fin<Unit>>(qty =>
                                            {
                                                if(qty < 0)
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

                                        if (fill.Direction == Direction.LONG)
                                        {
                                            return
                                            position.IncreaseLeg(leg.Id, fill.Quantity, fill.Price)
                                           
                                            .Bind<Fin<Unit>>(_ => order.UpdateFillStatus(fill.Id, FillStatus.FullyApplied))
                                            .Bind<Fin<Unit>>(_ => ProcessBookAllocation(globalBook, exchangeBook, legBook, product, order.Direction, fill.Quantity, fill.Price));  //book.GetBookEntryByProductId(product.Id), $"Unknown direction for fill: {fill.Id}")
                                        }
                                        else if (fill.Direction == Direction.SHORT)
                                        {
                                            return position.DecreaseLeg(leg.Id, fill.Quantity)
                                                .Bind<Fin<Unit>>(_ => order.UpdateFillStatus(fill.Id, FillStatus.FullyApplied))
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

                                    //update position status
                                    var _ = position.UpdateStatusBasedOnIntents();

                                    await unitOfWork.Begin();
                                    var positionRes = positionService.UpdateAsync(position);
                                    var orderRes = orderService.UpdateAsync(order);
                                    var globalBookUpdateRes = bookService.UpdateAsync(globalBook);
                                    var legBookUpdateRes = bookService.UpdateAsync(legBook);
                                    var exchangeBookUpdateRes = bookService.UpdateAsync(exchangeBook);
                                    var taskList = new List<Task<Fin<Unit>>> { positionRes, orderRes, globalBookUpdateRes, legBookUpdateRes, exchangeBookUpdateRes };
                                    var res = await Task.WhenAll(taskList);

                                    if (res.All(result => result.IsSucc))
                                    {
                                        await unitOfWork.Commit();
                                        return FinSucc(Unit.Default);
                                    }
                                    else
                                    {
                                        await unitOfWork.Rollback();
                                        return FinFail<Unit>(string.Join(", ", res.Fails().Select(r => r.Message)));
                                    }
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
            var position = await positionService.GetPositionForOrderIdAsync(order.Id);
            return await position.Match(
                Succ: async position => await ProcessPosition(position, order),
                Fail: error => Task.FromResult(FinFail<Unit>(error))
            );
        }

        public async Task<Fin<Unit>> UpdatePositions()
        {
            // Get all orders with unhandled fills
            var orders = await orderService.GetOrdersWithUnhandledFills();

            return await orders.Match(
                async orders =>
                {
                    foreach (var order in orders.OrderBy(o => o.ExchangeTimestamp))
                    {
                        var result = await ProcessOrder(order);

                        if (result.IsFail)
                        {
                            return FinFail<Unit>($"Failed to process order {order.Id}");
                        }
                    }

                    return FinSucc(Unit.Default);
                },
                error => Task.FromResult(FinFail<Unit>(error))
            );
        }

        //public async Task<Fin<Unit>> UpdatePositions()
        //{
        //    // Get all orders with unhandled fills

        //    //TODO: When supporting limit orders we need to loop over fills and get the order for the fill
        //    //This is due to the fact that we need to allocate the books in the right order. 
        //    //However book allocation might be splitted out into a different service.

        //    var orders = await orderService.GetOrdersWithUnhandledFills();

        //    // Get all positions for the orders

        //    //    var orderIds = orders.OrderBy(x => x.ExchangeTimestamp).Select(o => o.Id.InternalId).ToList();

        //    //Get grouped positions for the orders
        //    //Note that a position can be for multiple orders, but one order can only have one position
        //    //That will change when intent based volume mapping is added
        //    //But that means that we now can loop over the orders, and apply all the fills for that order to the position

        //    // Process each order asynchronously
        //    return await orders.Match(
        //        async orders =>
        //        {
        //            var tasks = orders.Select(async order => await ProcessOrder(order));

        //            var results = await Task.WhenAll(tasks);

        //            return results.FlattenResult();
        //        },
        //        error => Task.FromResult(FinFail<Unit>(error))
        //    );

        //}
    }
}

