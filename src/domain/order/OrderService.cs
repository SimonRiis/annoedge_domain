using domain.shared.repositories;
using LanguageExt;
using static LanguageExt.Prelude;

namespace domain.order
{
    public class OrderService(IUnitOfWork unitOfWork, IOrderRepository orderRepository, IOrderStatusRepository orderStatusRepository, IFillStatusRepository fillStatusRepository, IFillRepository fillRepository, ITradeRepository tradeRepository) : IOrderService
    {
        public Task<Fin<IEnumerable<Order>>> GetOrdersWithUnhandledFills()
        {
            return orderRepository.GetOrdersWithUnhandledFills();
        }

        private async Task<Fin<Unit>> UpdateFill(Fill fill)
        {
            //var fillRes = await fillRepository.UpdateAsync(fill); // This is not needed as we are not updating the fill itself TODO clean up

            //if (fillRes.IsFail)
            //    return fillRes;

            var fillStatusRes = await fillStatusRepository.UpdateForFillId(fill.Id, fill.Status);

            if (fillStatusRes.IsFail)
                return fillStatusRes;

            return FinSucc(Unit.Default);
        }

        private async Task<Fin<Unit>> UpdateFills(Order order)
        {

            foreach(var fill in order.Fills)
            {
                var fillRes = await UpdateFill(fill);
                if (fillRes.IsFail)
                    return fillRes;
            }

            return FinSucc(Unit.Default);

            //return Task.WhenAll(order.Fills.Select(UpdateFill)).ContinueWith(task =>
            //{
            //    if (task.Result.All(r => r.IsSucc))
            //        return FinSucc(Unit.Default);
            //    else
            //        return FinFail<Unit>(string.Join(", ", task.Result.Where(r => r.IsFail).Select(r => r.ToString())));
            //});
        }

        public async Task<Fin<Unit>> UpdateAsync(Order order)
        {
            //var orderRes = await orderRepository.UpdateAsync(order); // This is not needed as we are not updating the order itself TODO clean up

            //if (orderRes.IsFail)
            //    return orderRes;

            var orderStatusRes = await orderStatusRepository.UpdateForOrderId(order.Id, order.Status);

            if (orderStatusRes.IsFail)
                return orderStatusRes;

            var fillsRes = await UpdateFills(order);

            if (fillsRes.IsFail)
                return fillsRes;

            return FinSucc(Unit.Default);


        }

        public async Task<Fin<Unit>> AddAsync(Order order)
        {
            try
            {
                await unitOfWork.Begin();
                var orderRes = await orderRepository.AddAsync(order);

                if (orderRes.IsFail)
                {
                    await unitOfWork.Rollback();
                    return orderRes;
                }

                var orderStatusRes = await orderStatusRepository.InitOrderStatus(order.Id);

                if (orderStatusRes.IsFail)
                {
                    await unitOfWork.Rollback();
                    return orderStatusRes;
                }

                foreach (var fill in order.Fills)
                {
                    var fillRes = await fillRepository.AddAsync(fill);

                    if (fillRes.IsFail)
                    {
                        await unitOfWork.Rollback();
                        return fillRes;
                    }

                    var fillStatusRes = await fillStatusRepository.InitFillStatus(fill.Id);

                    if (fillStatusRes.IsFail)
                    {
                        await unitOfWork.Rollback();
                        return fillStatusRes;
                    }
                }

                foreach (var trade in order.Trades)
                {
                    var tradeRes = await tradeRepository.AddAsync(trade);

                    if (tradeRes.IsFail)
                    {
                        await unitOfWork.Rollback();
                        return tradeRes;
                    }
                }

                await unitOfWork.Commit();
                return FinSucc(Unit.Default);

            }
            catch (Exception ex)
            {
                await unitOfWork.Rollback();
                return FinFail<Unit>(ex.Message);
            }

            ////var fillsRes = Task.WhenAll(order.Fills.Select(fillRepository.AddAsync)).ContinueWith(task =>
            ////{
            ////    if (task.Result.All(r => r.IsSucc))
            ////        return FinSucc(Unit.Default);
            ////    else
            ////        return FinFail<Unit>(string.Join(", ", task.Result.Where(r => r.IsFail).Select(r => r.ToString())));
            ////});
            //var tradesRes = Task.WhenAll(order.Trades.Select(tradeRepository.AddAsync)).ContinueWith(task =>
            //{
            //    if (task.Result.All(r => r.IsSucc))
            //        return FinSucc(Unit.Default);
            //    else
            //        return FinFail<Unit>(string.Join(", ", task.Result.Where(r => r.IsFail).Select(r => r.ToString())));
            //});

            //var tasks = new List<Task<Fin<Unit>>> { orderRes, fillsRes, tradesRes };
            //return Task.WhenAll(tasks).ContinueWith(task =>
            //{
            //    return task.Result.FlattenResult();
            //});
        }
    }
}
