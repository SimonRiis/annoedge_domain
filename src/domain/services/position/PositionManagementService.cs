using domain.order;
using domain.position;
using LanguageExt;
using LanguageExt.Common;
using static LanguageExt.Prelude;

namespace domain.services.position
{
    public class PositionManagementService(IPositionService positionService, IPositionOrderMapRepository positionOrderMapRepository, IPositionRepository positionRepository,
        IPositionLegRepository positionLegRepository, IOrderRepository orderRepository, IFillRepository fillRepository)
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
                Succ: positionRepository.UpdateAsync,
                Fail: error => Task.FromResult(FinFail<Unit>(error))
            );
        }

        public async Task<Fin<Unit>> ImportPositions()
        {

            //get open positions

            var openPositionsRes = await positionRepository.QueryAsync("select * from position where status != 'CLOSED'");

            return await openPositionsRes.Match(
                Succ: async positions =>
                {
                    var tasks = positions.Select(ProcessPosition);
                    var results = await Task.WhenAll(tasks);
                    return results.All(result => result.IsSucc)
                        ? FinSucc(Unit.Default)
                        : FinFail<Unit>(string.Join(", ", results.Fails().Select(r => r.Message)));
                },
                Fail: error => Task.FromResult(FinFail<Unit>(error))
            );

            //var openPositionsRes = await positionRepository.QueryAsync("select * from position where status != 'CLOSED'");

            //return await openPositionsRes.Match(
            //    Succ: async positions =>
            //    {
            //        var tasks = positions.Select(async position =>
            //        {
            //            var positionOrderMapRes = await positionOrderMapRepository.QueryAsync($"select * from position_order_map where position_id = '{position.Id}'");

            //            return await positionOrderMapRes.Match(
            //                Succ: async positionOrderMaps =>
            //                {
            //                    var orders = positionOrderMaps.Map(pom => pom.OrderId).ToList();
            //                    var ordersRes = await orderRepository.QueryAsync($"select * from order where id in ({string.Join(",", orders)})");

            //                    return await ordersRes.Match(
            //                        Succ: orders =>
            //                        {
            //                            var sortedOrders = orders.OrderBy(order => order.ExchangeTimestamp).ToList();
            //                            var positionRes = positionService.UpdatePositionLegs(position, sortedOrders);

            //                            return positionRes.Match(
            //                                Succ: updatedPosition => positionRepository.UpdateAsync(updatedPosition),
            //                                Fail: error => Task.FromResult(FinFail<Unit>(error))
            //                            );
            //                        },
            //                        Fail: error => Task.FromResult(FinFail<Unit>(error))
            //                    );
            //                },
            //                Fail: error => Task.FromResult(FinFail<Unit>(error))
            //            );
            //        });

            //        var results = await Task.WhenAll(tasks);
            //        return results.All(result => result.IsSucc)
            //            ? FinSucc(Unit.Default)
            //            : FinFail<Unit>(string.Join(", ", results.Where(r => r.IsFail).Select(r => r.Throw().Message)));
            //    },
            //    Fail: error => Task.FromResult(FinFail<Unit>(error))
            //);


            //return openPositionsRes.Match(

            //    Succ: async positions => { 

            //        await Task.Delay(0);
            //        return Error.New("");
            //    },
            //    Fail: error => return Error.New("") //Console.WriteLine($"Error getting open positions: {error.Message}")
            //    );



            //openPositionsRes.Match(openPositionsRes =>
            //{
            //    openPositionsRes.Iter(async position =>
            //    {
            //        //get orders for position
            //        var positionOrderMapRes = await positionOrderMapRepository.QueryAsync($"select * from position_order_map where position_id = '{position.Id}'");
            //        positionOrderMapRes.Match(async positionOrderMapRes =>
            //        {
            //            var orders = positionOrderMapRes.Map(pom => pom.OrderId).ToList();
            //            var ordersRes = await orderRepository.QueryAsync($"select * from order where id in ({string.Join(",", orders)})");
            //            ordersRes.Match(ordersRes =>
            //            {
            //                var fills = ordersRes.Map(order => order.Fills).ToList();
            //                fills.Iter(async fill =>
            //                {
            //                    var fillRes = await fillRepository.AddAsync(fill);
            //                    fillRes.IfFail(ex => Console.WriteLine($"Error adding fill: {ex.Message}"));
            //                });
            //            }, ex => Console.WriteLine($"Error getting orders for position: {ex.Message}"));
            //        }, ex => Console.WriteLine($"Error getting position order map for position: {ex.Message}"));
            //    });
            //}, ex => Console.WriteLine($"Error getting open positions: {ex.Message}")


        }

    }
}
