using LanguageExt;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace domain.shared.repositories
{
    public interface IExternalDataRepository<T>
    {
        Task<Option<T>> GetByIdAsync(string id);
        Task<Fin<IEnumerable<T>>> QueryAsync(string query);
        Task<Fin<Unit>> AddAsync(T entity);
        Task<Fin<Unit>> UpdateAsync(T entity);
        Task<Fin<Unit>> DeleteAsync(string id);
    }
}
