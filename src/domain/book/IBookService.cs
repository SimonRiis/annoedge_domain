using domain.shared;
using LanguageExt;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace domain.book
{
    public interface IBookService
    {
        public Task<Option<Book>> GetForId(Guid id);
        public Task<Fin<Unit>> UpdateAsync(Book book);
    }
}
