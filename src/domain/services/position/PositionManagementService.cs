using domain.order;
using domain.position;
using LanguageExt;
using LanguageExt.Common;
using static LanguageExt.Prelude;

namespace domain.services.position
{
    public class PositionManagementService(IPositionService positionService, IPositionOrderMapRepository positionOrderMapRepository, IOrderRepository orderRepository)
    {
        private async Task<Fin<Unit>> ProcessPosition(Position position)
        {
            var positionOrderMapRes = await positionOrderMapRepository.QueryAsync($"select * from position_order_map where position_id = '{position.Id}'");
            return await positionOrderMapRes.Match(
                Succ: async positionOrderMaps => await ProcessPositionOrderMaps(position, positionOrderMaps),
                Fail: error => Task.FromResult(FinFail<Unit>(error))
            );
        }

        private async Task<Fin<Unit>> ProcessPositionOrderMaps(Position position, IEnumerable<PositionOrderMap> positionOrderMaps)
        {
            var orders = positionOrderMaps.Map(pom => pom.OrderId).ToList();
            var ordersRes = await orderRepository.QueryAsync($"select * from order where id in ({string.Join(",", orders)})");
            return await ordersRes.Match(
                Succ: async orders => await UpdatePositionWithOrders(position, orders),
                Fail: error => Task.FromResult(FinFail<Unit>(error))
            );
        }

        private async Task<Fin<Unit>> UpdatePositionWithOrders(Position position, IEnumerable<Order> orders)
        {
            var sortedOrders = orders.OrderBy(order => order.ExchangeTimestamp).ToList();
            var positionRes = positionService.UpdatePositionLegs(position, sortedOrders);
            return await positionRes.Match(
                Succ: positionService.UpdateAsync,
                Fail: error => Task.FromResult(FinFail<Unit>(error))
            );
        }

        public async Task<Fin<Unit>> ImportPositions()
        {
            var openPositionsRes = await positionService.GetPositions("select * from position where status != 'CLOSED'");

            return await openPositionsRes.Match(
                Succ: async positions =>
                {
                    var tasks = positions.Select(position => position.Bind(position => ProcessPosition(position)));
                    var results = await Task.WhenAll(tasks);
                    return results.All(result => result.IsSucc)
                        ? FinSucc(Unit.Default)
                        : FinFail<Unit>(string.Join(", ", results.Fails().Select(r => r.Message)));
                },
                Fail: error => Task.FromResult(FinFail<Unit>(error))
            );
        }

    }
}
