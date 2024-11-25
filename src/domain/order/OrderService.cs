using LanguageExt;
using static LanguageExt.Prelude;

namespace domain.order
{
    public class OrderService(IOrderRepository orderRepository, IFillRepository fillRepository, ITradeRepository tradeRepository) : IOrderService
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

        public Task<Fin<Unit>> AddAsync(Order order)
        {
            var orderRes = orderRepository.AddAsync(order);
            var fillsRes = Task.WhenAll(order.Fills.Select(fillRepository.AddAsync)).ContinueWith(task =>
            {
                if (task.Result.All(r => r.IsSucc))
                    return FinSucc(Unit.Default);
                else
                    return FinFail<Unit>(string.Join(", ", task.Result.Where(r => r.IsFail).Select(r => r.ToString())));
            });
            var tradesRes = Task.WhenAll(order.Trades.Select(tradeRepository.AddAsync)).ContinueWith(task =>
            {
                if (task.Result.All(r => r.IsSucc))
                    return FinSucc(Unit.Default);
                else
                    return FinFail<Unit>(string.Join(", ", task.Result.Where(r => r.IsFail).Select(r => r.ToString())));
            });

            var tasks = new List<Task<Fin<Unit>>> { orderRes, fillsRes, tradesRes };
            return Task.WhenAll(tasks).ContinueWith(task =>
            {
                return task.Result.FlattenResult();
            });
        }
    }
}
