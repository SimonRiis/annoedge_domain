using domain.shared.repositories;
using LanguageExt;

namespace domain.position;

public interface IPositionRepository : IRepository<Position>
{
    Task<Option<Position>> GetForOrderId(Guid id);
}
