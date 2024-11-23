using domain.book;
using LanguageExt;
using domain.shared;
using LanguageExt;
using static LanguageExt.Prelude;

namespace domain.order
{
    public class OrderService(IOrderRepository orderRepository, IFillRepository fillRepository) : IOrderService
    {
        public Task<Fin<IEnumerable<Order>>> GetOrdersWithUnhandledFills()
        {
            return orderRepository.GetOrdersWithUnhandledFills();
        }

        private Task<Fin<Unit>> UpdateFills(Order order)
        {
            return Task.WhenAll(order.Fills.Select(fillRepository.UpdateAsync)).ContinueWith(task =>
            {
                if (task.Result.All(r => r.IsSucc))
                    return FinSucc(Unit.Default);
                else
                    return FinFail<Unit>(string.Join(", ", task.Result.Where(r => r.IsFail).Select(r => r.ToString())));
            });
        }

        public async Task<Fin<Unit>> UpdateAsync(Order order)
        {
            var orderRes = orderRepository.UpdateAsync(order);
            var fillsRes = UpdateFills(order);
            var tasks = new List<Task<Fin<Unit>>> { orderRes, fillsRes };
            var results = await Task.WhenAll(tasks);

            return results.FlattenResult();
        }
    }
}
