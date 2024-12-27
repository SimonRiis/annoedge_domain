using domain.order;
using domain.shared.repositories;
using LanguageExt;
using LanguageExt.ClassInstances;
using LanguageExt.Common;
using System.Runtime.InteropServices;
using static LanguageExt.Prelude;

namespace domain.position
{
    public class PositionService(IUnitOfWork unitOfWork, IPositionLegService positionLegService, IPositionLegRepository positionLegRepository,
        IPositionRepository positionRepository, IPositionOrderMapRepository positionOrderMapRepository, IProductRepository productRepository) : IPositionService
    {
        public Fin<Position> CreatePosition(IEnumerable<Order> orders, PositionMandate positionMandate)
        {
            if (!orders.Any())
                return Error.New("Orders cannot be empty");

            if (positionMandate.Id == Guid.Empty)
                return Error.New("PositionMandate Id cannot be empty");

            if (positionMandate.StoplossPct <= 0)
                return Error.New("PositionMandate Stoploss cannot be 0 or negative");

            if (positionMandate.TakeProfitPct <= 0)
                return Error.New("PositionMandate Take profit cannot be 0 or negative");

            if (positionMandate.MaxPositionSize <= 0)
                return Error.New("PositionMandate max position size cannot be 0 or negative");

            var positionId = Guid.NewGuid();
            return FinSucc(new Position(positionId, PositionStatus.PENDING, DateTimeOffset.UtcNow, positionMandate, positionLegService)); ;
        }

        public Fin<Position> CreatePosition(Guid positionId, PositionMandate positionMandate)
        {
            if (positionId == Guid.Empty)
                return Error.New("Position Id cannot be empty");

            if (positionMandate.Id == Guid.Empty)
                return Error.New("PositionMandate Id cannot be empty");

            if (positionMandate.StoplossPct <= 0)
                return Error.New("PositionMandate Stoploss cannot be 0 or negative");

            if (positionMandate.TakeProfitPct <= 0)
                return Error.New("PositionMandate Take profit cannot be 0 or negative");

            if (positionMandate.MaxPositionSize <= 0)
                return Error.New("PositionMandate max position size cannot be 0 or negative");

            return FinSucc(new Position(positionId, PositionStatus.PENDING, DateTimeOffset.UtcNow, positionMandate, positionLegService));

        }


        private async Task<Fin<Position>> ProcessPosition(Position position)
        {
            var positionLegs = await positionLegRepository.GetAllForPosition(position.Id);
            return positionLegs.Match(
                Succ: legs =>
                {
                    legs.Iter(leg =>
                    {
                        position.AddLeg(leg);
                    });
                    return FinSucc(position);
                },
                Fail: error => FinFail<Position>(error)
            );
        }

        public async Task<Fin<IEnumerable<Fin<Position>>>> GetPositions(string query)
        {
            var positionsResult = await positionRepository.QueryAsync(query);

            return await Task.WhenAll(positionsResult.Bind(positions =>
            {
                return positions.Select(ProcessPosition);
            }
            ));
        }

        private Task<Fin<Unit>> UpdateLegs(Position position)
        {
            return Task.WhenAll(position.Legs.Select(positionLegRepository.UpdateAsync)).ContinueWith(task =>
            {
                if (task.Result.All(r => r.IsSucc))
                    return FinSucc(Unit.Default);
                else
                    return FinFail<Unit>(string.Join(", ", task.Result.Where(r => r.IsFail).Select(r => r.ToString())));
            });
        }

        private Task<Fin<Unit>> UpdateOrderMaps(Position position)
        {
            return Task.WhenAll(position.PositionOrderMaps.Select(positionOrderMapRepository.UpdateAsync)).ContinueWith(task =>
            {
                if (task.Result.All(r => r.IsSucc))
                    return FinSucc(Unit.Default);
                else
                    return FinFail<Unit>(string.Join(", ", task.Result.Where(r => r.IsFail).Select(r => r.ToString())));
            });
        }

        public async Task<Fin<Unit>> UpdateAsync(Position position)
        {
            var positionRes = positionRepository.UpdateAsync(position);
            var positionLegsRes = UpdateLegs(position);
            var positionOrderMapsRes = UpdateOrderMaps(position);
            var tasks = new List<Task<Fin<Unit>>> { positionRes, positionLegsRes, positionOrderMapsRes };
            var results = await Task.WhenAll(tasks);

            return results.FlattenResult();
        }
        public async Task<Fin<Unit>> AddAsync(Position position)
        {
            await unitOfWork.Begin();

            try
            {
                // Add the position
                var positionRes = await positionRepository.AddAsync(position);
                if (positionRes.IsFail)
                {
                    await unitOfWork.Rollback();
                    return positionRes;
                }

                // Add position legs sequentially
                foreach (var leg in position.Legs)
                {
                    var legRes = await positionLegRepository.AddAsync(leg);
                    if (legRes.IsFail)
                    {
                        await unitOfWork.Rollback();
                        return legRes;
                    }
                }

                // Add position order maps sequentially
                foreach (var orderMap in position.PositionOrderMaps)
                {
                    var mapRes = await positionOrderMapRepository.AddAsync(orderMap);
                    if (mapRes.IsFail)
                    {
                        await unitOfWork.Rollback();
                        return mapRes;
                    }
                }

                // Commit transaction if all operations succeed
                await unitOfWork.Commit();
                return FinSucc(Unit.Default);
            }
            catch (Exception ex)
            {
                await unitOfWork.Rollback();
                return FinFail<Unit>($"Unexpected error: {ex.Message}");
            }


            //await unitOfWork.Begin();
            //var positionRes = positionRepository.AddAsync(position);
            //var positionLegsRes = Task.WhenAll(position.Legs.Select(positionLegRepository.AddAsync)).ContinueWith(task =>
            //{
            //    if (task.Result.All(r => r.IsSucc))
            //        return FinSucc(Unit.Default);
            //    else
            //        return FinFail<Unit>(string.Join(", ", task.Result.Where(r => r.IsFail).Select(r => r.ToString())));
            //});

            //var positionOrderMapsRes = Task.WhenAll(position.PositionOrderMaps.Select(positionOrderMapRepository.AddAsync)).ContinueWith(task =>
            //{
            //    if (task.Result.All(r => r.IsSucc))
            //        return FinSucc(Unit.Default);
            //    else
            //        return FinFail<Unit>(string.Join(", ", task.Result.Where(r => r.IsFail).Select(r => r.ToString())));
            //});

            //var tasks = new List<Task<Fin<Unit>>> { positionRes, positionLegsRes, positionOrderMapsRes };
            //var results = await Task.WhenAll(tasks);

            //if (results.All(result => result.IsSucc))
            //{
            //    await unitOfWork.Commit();
            //    return FinSucc(Unit.Default);
            //}
            //else
            //{
            //    await unitOfWork.Rollback();
            //    return results.FlattenResult();
            //}
        }

        public async Task<Fin<Position>> GetPositionForOrderIdAsync(string orderId)
        {
            var positionRes = await positionRepository.GetForOrderId(orderId);
            return await positionRes.Match(
                Some: async position => { return await ProcessPosition(position); },
                None: () => Task.FromResult(FinFail<Position>($"Could not find position for order: {orderId}"))
            );
        }

        //public async Task<Fin<Unit>> MapOrderToPosition(Guid positionId, string orderId)
        //{
        //    if (positionId == Guid.Empty)
        //        return FinFail<Unit>("Position Id cannot be empty");

        //    if (string.IsNullOrEmpty(orderId))
        //        return FinFail<Unit>("Order Id cannot be empty");

        //    var position = await GetPositionAsync(positionId);

        //    return await position.Match(
        //        Succ: async p =>
        //        {
        //            var positionOrderMap = new PositionOrderMap { PositionId = positionId, OrderId = orderId };
        //            var pRes = p.AddPositionOrderMap(positionOrderMap);

        //            return await pRes.Match(

        //                Succ: async _ => { return await UpdateAsync(p); },
        //                Fail: error => Task.FromResult(FinFail<Unit>(error))

        //                );
        //        },
        //        Fail: error => Task.FromResult(FinFail<Unit>("Could not find position"))
        //    );
        //}

        public async Task<Fin<Position>> GetPositionAsync(Guid positionId)
        {
            var positionRes = await positionRepository.GetByIdAsync(positionId);
            return await positionRes.Match(
                Some: async position => { return await ProcessPosition(position); },
                None: () => Task.FromResult(FinFail<Position>($"Could not find position for id: {positionId}"))
            );
        }

        public async Task<Fin<IEnumerable<Fin<Position>>>> GetOpenPositionsForSymbolAndExchange(string symbol, Guid exchangeId)
        {
            var mappedSymbol = await productRepository.GetByAssetAndExchange(symbol, exchangeId);
            var product = mappedSymbol.ToFin("Product not found");
            return await product.BindAsync(async product =>
            {
                var positions = await positionRepository.GetAllOpenForProductOnExchange(product, exchangeId);
                return await positions.BindAsync(async p =>
                {
                    IEnumerable<Fin<Position>> positions = await Task.WhenAll(p.Select(ProcessPosition));
                    return FinSucc(positions);
                });
            });
        }
    }
}
