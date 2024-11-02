using System;
using LanguageExt;

namespace domain.shared.repositories;

public interface IRepository<T>
{
    Task<Option<T>> GetByIdAsync(Guid id);
    Task<Fin<IEnumerable<T>>> QueryAsync(string query);
    Task<Fin<Unit>> AddAsync(T position);
    Task<Fin<Unit>> UpdateAsync(T position);
    Task<Fin<Unit>> DeleteAsync(Guid id);
}
