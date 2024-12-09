using domain.shared.enums;
using domain.shared.repositories;
using LanguageExt;

namespace domain.order
{
    public interface IOrderStatusRepository : IRepository<Status>
    {
        Task<Fin<Status>> GetForOrderId(string orderId);
        Task<Fin<Unit>> UpdateForOrderId(string orderId, Status status);
    }
}
