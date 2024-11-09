using domain.order;
using LanguageExt;
using LanguageExt.Common;
using static LanguageExt.Prelude;
namespace domain.services.order
{
    public class OrderService : IOrderService
    {
        public Fin<Order> CreateFrom(OrderDTO order)
        {
            var orderAggregate = new Order(order.Product, order.Exchange, order.Id, order.Quantity, order.Price, order.Direction, order.ExchangeTimestamp, order.LastUpdated);

            var tradesRes = order.Trades.Map(trade =>
            {
                var tradeEntity = new Trade
                {
                    ExchangeTimestamp = trade.ExchangeTimestamp,
                    Id = trade.Id,
                    OrderId = trade.OrderId,
                    Price = trade.Price,
                    Quantity = trade.Quantity,
                    TotalPriceInUSD = trade.TotalPriceInUSD,
                    Direction = trade.Direction
                };

                return orderAggregate.AddTrade(tradeEntity);
            });

            if (tradesRes.Fails().Any())
            {
                return Error.New(String.Join(", ", tradesRes.Fails().Select(x => x.Message)));
            }

            var orderRes = order.Fills.Map(fill =>
            {
                var fillEntity = new Fill
                {
                    ExchangeTimestamp = fill.ExchangeTimestamp,
                    Id = fill.Id,
                    OrderId = fill.OrderId,
                    Price = fill.Price,
                    Quantity = fill.Quantity,
                    Direction = fill.Direction
                };
                return orderAggregate.AddFill(fillEntity);
            });

            if (orderRes.Fails().Any())
            {
                return Error.New(String.Join(", ", orderRes.Fails().Select(x => x.Message)));
            }

            return FinSucc(orderAggregate);
        }
    }
}
