using domain.globalbook;
using domain.order;
using LanguageExt;

namespace domain.services.book
{
    public interface IGlobalBookService
    {
        Task<Fin<GlobalBook>> CreateFrom(IEnumerable<Order> orders, CancellationToken cancellationToken);
    }
}
