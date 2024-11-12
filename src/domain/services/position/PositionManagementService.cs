using domain.order;
using domain.position;
using domain.shared;
using domain.shared.enums;
using domain.shared.repositories;
using LanguageExt;
using LanguageExt.Common;
using static LanguageExt.Prelude;

namespace domain.services.position
{
    public class PositionManagementService(IOrderService orderService, IPositionService positionService, IUnitOfWork unitOfWork) : IPositionManagementService
    {

        private async Task<Fin<Unit>> ProcessPosition(Position position, Order order) {

            var fillsForPosition = order.Fills.OrderBy(x => x.ExchangeTimestamp);
            var exchange = order.Exchange;
            var product = order.Product;
            var leg = position.Legs.FirstOrDefault(l => l.Product.Id == product.Id && l.Exchange.Id == exchange.Id);

            if (leg == null)
            {
                return Error.New($"Could not find leg: productId: {product.Id} and Exchange: {exchange.Id}");
            }

            fillsForPosition.Iter(fill =>
            {
                if (fill.Direction == Direction.LONG)
                {
                    position.IncreaseLeg(leg.Id, fill.Quantity, fill.Price);
                    order.UpdateFillStatus(fill.Id.InternalId, FillStatus.FullyApplied); //Fill is now fully applied
                }
                else if (fill.Direction == Direction.SHORT)
                {
                    position.DecreaseLeg(leg.Id, fill.Quantity);
                    order.UpdateFillStatus(fill.Id.InternalId, FillStatus.FullyApplied); //Fill is now fully applied
                }
            });


            await unitOfWork.Begin();
            var positionRes = positionService.UpdateAsync(position);
            var orderRes = orderService.UpdateAsync(order);
            var taskList = new List<Task<Fin<Unit>>> { positionRes, orderRes };
            var res = await Task.WhenAll(taskList);

            if(res.All(result => result.IsSucc))
            {
                await unitOfWork.Commit();
                return FinSucc(Unit.Default);
            }
            else
            {
                await unitOfWork.Rollback();
                return FinFail<Unit>(string.Join(", ", res.Fails().Select(r => r.Message)));
            }

        }

        private async Task<Fin<Unit>> ProcessOrder(Order order)
        {
            var position = await positionService.GetPositionForOrderIdAsync(order.Id.InternalId);
            return await position.Match(
                Succ: async position => await ProcessPosition(position, order),
                Fail: error => Task.FromResult(FinFail<Unit>(error))
            );
        }

        public async Task<Fin<Unit>> UpdatePositions()
        {
            // Get all orders with unhandled fills
            var orders = await orderService.GetOrdersWithUnhandledFills();

            // Get all positions for the orders

            //    var orderIds = orders.OrderBy(x => x.ExchangeTimestamp).Select(o => o.Id.InternalId).ToList();

            //Get grouped positions for the orders
            //Note that a position can be for multiple orders, but one order can only have one position
            //That will change when intent based volume mapping is added
            //But that means that we now can loop over the orders, and apply all the fills for that order to the position

            // Process each order asynchronously
            return await orders.Match(
                async orders =>
                {
                    var tasks = orders.Select(async order =>
                        await order.Match(
                            async o => await ProcessOrder(o),
                            error => Task.FromResult(FinFail<Unit>(error))
                        )
                    );

                    var results = await Task.WhenAll(tasks);

                    return results.All(result => result.IsSucc)
                        ? FinSucc(Unit.Default)
                        : FinFail<Unit>(string.Join(", ", results.Fails().Select(r => r.Message)));
                },
                error => Task.FromResult(FinFail<Unit>(error))
            );

        }
    }
}

