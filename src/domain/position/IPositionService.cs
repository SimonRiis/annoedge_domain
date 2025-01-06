using domain.order;
using domain.shared;
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
        Task<Fin<IEnumerable<Fin<Position>>>> GetOpenPositions();
        Task<Fin<IEnumerable<Fin<Position>>>> GetOpenPositionsForSymbolAndExchange(string symbol, Guid exchangeId);

        Task<Fin<PositionPnLEvent>> GetLatestPnlEvent(Guid positionId);
        Task<Fin<PositionEvent>> GetLatestEvent(Guid positionId);
    }
}
