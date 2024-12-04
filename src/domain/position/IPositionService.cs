using domain.order;
using LanguageExt;

namespace domain.position
{
    public interface IPositionService
    {
        Task<Fin<Position>> GetPositionForOrderIdAsync(string orderId);
        Task<Fin<Position>> GetPositionAsync(Guid positionId);
        Task<Fin<Unit>> UpdateAsync(Position position);
        Task<Fin<Unit>> AddAsync(Position position);

        Fin<Position> CreatePosition(IEnumerable<Order> orders, PositionMandate positionMandate);

        Fin<Position> CreatePosition(Guid positionId, PositionMandate positionMandate);
        Task<Fin<IEnumerable<Fin<Position>>>> GetPositions(string query);
    }
}
