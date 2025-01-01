using domain.shared.repositories;
using LanguageExt;

namespace domain.order
{
    public interface IFillRepository : IExternalDataRepository<Fill>
    {
        Task<Fin<IEnumerable<Fill>>> GetFillsForOrderId(string orderId);
    }
}
