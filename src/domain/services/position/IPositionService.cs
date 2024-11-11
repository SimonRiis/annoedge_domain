using domain.order;
using domain.position;
using LanguageExt;

namespace domain.services.position
{
    public interface IPositionService
    {
        Fin<Position> UpdatePositionLegs(Position position, IEnumerable<Order> orders);
        Task<Fin<Unit>> UpdateAsync(Position position);

        Fin<Position> CreatePosition(IEnumerable<Order> orders, PositionMandate positionMandate);

        Fin<Position> CreatePosition(Guid positionId, PositionMandate positionMandate);

        Task<Fin<IEnumerable<Fin<Position>>>> GetPositions(string query);
    }
}
