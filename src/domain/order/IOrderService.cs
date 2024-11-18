using LanguageExt;

namespace domain.order
{
    public interface IOrderService
    {
        Task<Fin<IEnumerable<Order>>> GetOrdersWithUnhandledFills();

        Task<Fin<Unit>> UpdateAsync(Order order);
    }
}
