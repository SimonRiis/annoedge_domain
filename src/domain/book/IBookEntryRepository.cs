using domain.product;
using domain.shared.repositories;
using LanguageExt;

namespace domain.book
{
    public interface IBookEntryRepository : IRepository<BookEntry>
    {
        Task<Option<BookEntry>> GetForProductId(Product product);
    }
}
