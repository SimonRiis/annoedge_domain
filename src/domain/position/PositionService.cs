using domain.order;
using LanguageExt;
using LanguageExt.Common;
using static LanguageExt.Prelude;

namespace domain.position
{
    public class PositionService(IPositionLegService positionLegService, IPositionLegRepository positionLegRepository, IPositionRepository positionRepository) : IPositionService
    {
        public Fin<Position> CreatePosition(IEnumerable<Order> orders, PositionMandate positionMandate)
        {
            if (!orders.Any())
                return Error.New("Orders cannot be empty");

            if (positionMandate.Id == Guid.Empty)
                return Error.New("PositionMandate Id cannot be empty");

            if (positionMandate.StoplossPct >= 0)
                return Error.New("PositionMandate Stoploss cannot be 0 or positive");

            if (positionMandate.TakeProfitPct <= 0)
                return Error.New("PositionMandate Take profit cannot be 0 or negative");

            if (positionMandate.MaxPositionSize <= 0)
                return Error.New("PositionMandate max position size cannot be 0 or negative");

            var positionId = Guid.NewGuid();
            return FinSucc(new Position(positionId, PositionStatus.PENDING, DateTimeOffset.UtcNow, positionMandate, positionLegService)); ;
        }

        public Fin<Position> CreatePosition(Guid positionId, PositionMandate positionMandate)
        {
            if (positionId == Guid.Empty)
                return Error.New("Position Id cannot be empty");

            if (positionMandate.Id == Guid.Empty)
                return Error.New("PositionMandate Id cannot be empty");

            if (positionMandate.StoplossPct >= 0)
                return Error.New("PositionMandate Stoploss cannot be 0 or positive");

            if (positionMandate.TakeProfitPct <= 0)
                return Error.New("PositionMandate Take profit cannot be 0 or negative");

            if (positionMandate.MaxPositionSize <= 0)
                return Error.New("PositionMandate max position size cannot be 0 or negative");

            return FinSucc(new Position(positionId, PositionStatus.PENDING, DateTimeOffset.UtcNow, positionMandate, positionLegService));

        }


        private async Task<Fin<Position>> ProcessPosition(Position position)
        {
            var positionLegs = await positionLegRepository.GetAllForPosition(position.Id);
            return positionLegs.Match(
                Succ: legs =>
                {
                    legs.Iter(leg =>
                    {
                        position.AddLeg(leg);
                    });
                    return FinSucc(position);
                },
                Fail: error => FinFail<Position>(error)
            );
        }

        public async Task<Fin<IEnumerable<Fin<Position>>>> GetPositions(string query)
        {
            var positionsResult = await positionRepository.QueryAsync(query);

            return await Task.WhenAll(positionsResult.Bind(positions =>
            {
                return positions.Select(ProcessPosition);
            }
            ));
        }

        private Task<Fin<Unit>> UpdateLegs(Position position)
        {
            return Task.WhenAll(position.Legs.Select(positionLegRepository.UpdateAsync)).ContinueWith(task =>
            {
                if (task.Result.All(r => r.IsSucc))
                    return FinSucc(Unit.Default);
                else
                    return FinFail<Unit>(string.Join(", ", task.Result.Where(r => r.IsFail).Select(r => r.ToString())));
            });
        }

        public async Task<Fin<Unit>> UpdateAsync(Position position)
        {
            var positionRes = positionRepository.UpdateAsync(position);
            var positionLegsRes = UpdateLegs(position);
            var tasks= new List<Task<Fin<Unit>>> { positionRes, positionLegsRes };
            var results = await Task.WhenAll(tasks);

            return results.FlattenResult();
        }

        public async Task<Fin<Position>> GetPositionForOrderIdAsync(string orderId)
        {
            var positionRes = await positionRepository.GetForOrderId(orderId);
            return await positionRes.Match(
                Some: async position => { return await ProcessPosition(position);},
                None: () => Task.FromResult(FinFail<Position>($"Could not find position for order: {orderId}"))
            );
        }
    }
}
