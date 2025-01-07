using domain.exchange;
using domain.order;
using domain.product;
using domain.shared.repositories;
using LanguageExt;
using LanguageExt.ClassInstances;
using LanguageExt.Common;
using System.Runtime.InteropServices;
using static LanguageExt.Prelude;

namespace domain.position
{
    public class PositionService(IUnitOfWork unitOfWork, IPositionLegService positionLegService, IPositionLegRepository positionLegRepository,
        IPositionRepository positionRepository, IPositionOrderMapRepository positionOrderMapRepository, IPositionEventService positionEventService,
        IPositionPnlEventService positionPnlEventService, IProductRepository productRepository, IPositionLegIntentRepository positionLegIntentRepository) : IPositionService
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

            if (positionMandate.CurrentlyAllocated <= 0)
                return Error.New("PositionMandate max position size cannot be 0 or negative");

            if(positionMandate.MaxAllocated <= 0)
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

            if (positionMandate.CurrentlyAllocated <= 0)
                return Error.New("PositionMandate max position size cannot be 0 or negative");

            if (positionMandate.MaxAllocated <= 0)
                return Error.New("PositionMandate max position size cannot be 0 or negative");

            return FinSucc(new Position(positionId, PositionStatus.PENDING, DateTimeOffset.UtcNow, positionMandate, positionLegService));

        }

        private async Task<Fin<Position>> ProcessPosition(Position position)
        {
            var positionLegs = await positionLegRepository.GetAllForPosition(position.Id);
            return await positionLegs.BindAsync(async legs =>
            {
                foreach (var leg in legs)
                {
                    var intents = await positionLegIntentRepository.GetAllForPositionLeg(leg.Id);

                    var res = intents.Bind(intents =>
                    {
                        foreach (var intent in intents)
                        {
                            var res = leg.AddIntent(intent);
                            if (res.IsFail)
                                return FinFail<PositionLeg>($"Error adding intent to leg: {leg.Id}");
                        }
                        return FinSucc(leg);
                    });

                    if (res.IsFail)
                        return FinFail<Position>($"Error adding legintents to position leg: {leg.Id} for position: {position.Id}");

                    var resLeg = position.AddLeg(leg);
                    if (resLeg.IsFail)
                        return FinFail<Position>($"Error adding leg to position: {position.Id}");

                }

                

                //var events = await positionEventRepository.GetAllForPosition(position.Id);

                //return await events.BindAsync(async events =>
                //{
                //    foreach (var ev in events)
                //    {
                //        var res = position.AddEvent(ev.EventType, ev.OldStatus, ev.NewStatus, ev.EventData);
                //        if (res.IsFail)
                //            return FinFail<Position>($"Error adding event to position: {position.Id}");
                //    }

                //    var pnlEvents = await positionPnlEventRepository.GetAllForPosition(position.Id);

                //    return pnlEvents.Bind(pnlEvents =>
                //    {
                //        foreach (var pnlEvent in pnlEvents)
                //        {
                //            var res = position.AddPnLEvent(pnlEvent.Pnl, pnlEvent.PnlPct);
                //            if (res.IsFail)
                //                return FinFail<Position>($"Error adding pnl event to position: {position.Id}");
                //        }

                //        return FinSucc(position);
                //    });
                //});

                return FinSucc(position);
            });
        }

        private async Task<Fin<Unit>> UpdateLegIntents(Position position)
        {
            foreach (var leg in position.Legs)
            {
                foreach (var intent in leg.PositionLegIntents)
                {
                    var res = await positionLegIntentRepository.UpdateAsync(intent);
                    if (res.IsFail)
                        return res;
                }
            }

            return FinSucc(Unit.Default);
        }

        private async Task<Fin<Unit>> AddLegIntents(Position position)
        {
            foreach (var leg in position.Legs)
            {
                foreach (var intent in leg.PositionLegIntents)
                {
                    var res = await positionLegIntentRepository.AddAsync(intent);
                    if (res.IsFail)
                        return res;
                }
            }

            return FinSucc(Unit.Default);
        }

        private async Task<Fin<Unit>> UpdateLegs(Position position)
        {

            foreach (var leg in position.Legs)
            {
                var res = await positionLegRepository.UpdateAsync(leg);
                if (res.IsFail)
                    return res;
            }

            return FinSucc(Unit.Default);
        }

        private async Task<Fin<Unit>> UpdateEvents(Position position)
        {
            foreach (var ev in position.Events)
            {
                var res = await positionEventService.SaveEvent(ev);
                if (res.IsFail)
                    return res;
            }

            return FinSucc(Unit.Default);
        }

        private async Task<Fin<Unit>> UpdatePnlEvents(Position position)
        {
            foreach (var pnlEvent in position.PnlEvents)
            {
                var res = await positionPnlEventService.SavePnlEvent(pnlEvent);
                if (res.IsFail)
                    return res;
            }

            return FinSucc(Unit.Default);
        }

        private async Task<Fin<Unit>> UpdateOrderMaps(Position position)
        {
            foreach (var orderMap in position.PositionOrderMaps)
            {
                var res = await positionOrderMapRepository.AddAsync(orderMap);
                if (res.IsFail)
                    return res;
            }
            return FinSucc(Unit.Default);

        }

        public async Task<Fin<Unit>> UpdateAsync(Position position)
        {
            var positionRes = await positionRepository.UpdateAsync(position);

            if (positionRes.IsFail)
                return positionRes;

            var positionLegIntentsRes = await UpdateLegIntents(position);

            if (positionLegIntentsRes.IsFail)
                return positionLegIntentsRes;

            var positionEventsRes = await UpdateEvents(position);
            if (positionEventsRes.IsFail)
                return positionEventsRes;

            var positionPnlEventsRes = await UpdatePnlEvents(position);

            if (positionPnlEventsRes.IsFail)
                return positionPnlEventsRes;

            var positionLegsRes = await UpdateLegs(position);

            if (positionLegsRes.IsFail)
                return positionLegsRes;

            var positionOrderMapsRes = await UpdateOrderMaps(position);

            if (positionOrderMapsRes.IsFail)
                return positionOrderMapsRes;

            return FinSucc(Unit.Default);

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

                var positionLegIntentsRes = await AddLegIntents(position);
                if (positionLegIntentsRes.IsFail)
                {
                    await unitOfWork.Rollback();
                    return positionLegIntentsRes;
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

        }

        public async Task<Fin<Position>> GetPositionForOrderIdAsync(string orderId)
        {
            var positionRes = await positionRepository.GetForOrderId(orderId);
            return await positionRes.Match(
                Some: async position => { return await ProcessPosition(position); },
                None: () => Task.FromResult(FinFail<Position>($"Could not find position for order: {orderId}"))
            );
        }

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
                    List<Fin<Position>> positions = [];
                    foreach (var position in p)
                    {
                        positions.Add(await ProcessPosition(position));
                    }

                    return FinSucc(positions.AsEnumerable());
                });
            });
        }

        public async Task<Fin<IEnumerable<Fin<Position>>>> GetOpenPositions()
        {
            var positions = await positionRepository.GetAllOpen();
            return await positions.BindAsync(async p =>
            {
                List<Fin<Position>> positions = [];
                foreach (var position in p)
                {
                    positions.Add(await ProcessPosition(position));
                }

                return FinSucc(positions.AsEnumerable());
            });
        }

     

       
    }
}
