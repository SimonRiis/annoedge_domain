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

        public async Task ImportPositions()
        {
            //get all open positions
            var positions = await positionRepository.QueryAsync("");

            positions.Match(
                Succ: async (positions) =>
                {
                    foreach (var position in positions)
                    {
                        //get all orders for the position
                        var orders = await positionOrderMapRepository.QueryAsync("position.Id");

                        orders.Bind(async (orders) =>
                        {
                            //update the position legs
                            var updatedPosition = await positionService.UpdatePositionLegs(position, orders);

                            //update the position
                            await positionRepository.UpdateAsync(updatedPosition);
                        });

                        ////update the position legs
                        //var updatedPosition = await positionService.UpdatePositionLegs(position, orders);

                        ////update the position
                        //await positionRepository.UpdateAsync(updatedPosition);

                    }
                    //return Fin<Unit>.Succ(unit);
                },
                Fail: (error) => Console.WriteLine(error.Message)
            );



        }

    }
}
