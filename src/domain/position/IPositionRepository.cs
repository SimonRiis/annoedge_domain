using domain.product;
using domain.shared.repositories;
using LanguageExt;

namespace domain.position;

public interface IPositionRepository : IRepository<Position>
{
    Task<Option<Position>> GetForOrderId(string id);

    Task<Fin<IEnumerable<Position>>> GetAllOpenForProductOnExchange(Product product, Guid exchangeId);
    Task<Fin<IEnumerable<Position>>> GetAllOpen();
    Task<Fin<IEnumerable<Position>>> GetOldOpenPositions(DateTimeOffset date);
}
