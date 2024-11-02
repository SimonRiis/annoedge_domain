using System;
using LanguageExt;

namespace domain.shared.repositories;

public interface IRepository<T>
{
    Task<Option<T>> GetByIdAsync(Guid id);
    Task<IEnumerable<T>> QueryAsync(string query);
    Task AddAsync(T position);
    Task UpdateAsync(T position);
    Task DeleteAsync(Guid id);
}
