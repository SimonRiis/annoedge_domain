using domain.algobook;
using domain.order;
using LanguageExt;

namespace domain.services.book
{
    public interface IAlgoBooksService
    {
        Task<Fin<IEnumerable<AlgoBook>>> CreateFrom(IEnumerable<Order> orders, CancellationToken cancellationToken);
    }
}
