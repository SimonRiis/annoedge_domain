using System;
using System.Linq.Expressions;
using LanguageExt;

namespace domain.position;

public interface IPositionRepository
{
    Task<Option<Position>> GetByIdAsync(Guid id);
    Task<IEnumerable<Position>> QueryAsync(Expression<Func<Position, bool>> filter);
    Task AddAsync(Position position);
    Task UpdateAsync(Position position);
    Task DeleteAsync(Guid id);
}
