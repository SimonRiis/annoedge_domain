using domain.shared;
using domain.shared.repositories;
using LanguageExt;

namespace domain.position;

public interface IPositionRepository : IRepository<Position>
{
    Task<Option<Position>> GetForOrderId(string id);

    Task<Fin<IEnumerable<Position>>> GetAllOpenForProductOnExchange(Product product, Guid exchangeId);
}
