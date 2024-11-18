using domain.order;
using LanguageExt;

namespace domain.position
{
    public interface IPositionService
    {
        Task<Fin<Position>> GetPositionForOrderIdAsync(Guid orderId);
        Task<Fin<Unit>> UpdateAsync(Position position);

        Fin<Position> CreatePosition(IEnumerable<Order> orders, PositionMandate positionMandate);

        Fin<Position> CreatePosition(Guid positionId, PositionMandate positionMandate);

        Task<Fin<IEnumerable<Fin<Position>>>> GetPositions(string query);
    }
}
