using domain.order;
using domain.position;
using domain.shared;
using domain.shared.enums;
using domain.shared.services;
using LanguageExt;
using LanguageExt.Common;
using System.Reflection.Metadata.Ecma335;
using static LanguageExt.Prelude;

namespace domain.services.position
{
    public class PositionService(IPositionLegService positionLegService, IPositionLegRepository positionLegRepository, IPositionRepository positionRepository) : IPositionService
    {
        public Fin<Position> CreatePosition(IEnumerable<Order> orders, PositionMandate positionMandate)
        {
            if (!orders.Any())
                return Error.New("Orders cannot be empty");

            if (positionMandate.Id == Guid.Empty)
                return Error.New("PositionMandate Id cannot be empty");

            if (positionMandate.StoplossPct >= 0)
                return Error.New("PositionMandate Stoploss cannot be 0 or positive");

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

            if (positionMandate.StoplossPct >= 0)
                return Error.New("PositionMandate Stoploss cannot be 0 or positive");

            if (positionMandate.TakeProfitPct <= 0)
                return Error.New("PositionMandate Take profit cannot be 0 or negative");

            if (positionMandate.MaxPositionSize <= 0)
                return Error.New("PositionMandate max position size cannot be 0 or negative");

            return FinSucc(new Position(positionId, PositionStatus.PENDING, DateTimeOffset.UtcNow, positionMandate, positionLegService));

        }


        private async Task<Fin<Position>> ProcessPosition(Position position)
        {
            var positionLegs = await positionLegRepository.QueryAsync($"select * from position_leg where position_id ={position.Id}");
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

        public Fin<Position> UpdatePositionLegs(Position position, IEnumerable<Order> orders)
        {
            var groupedOrders = orders
                .GroupBy(order => order.Exchange.Id)
                .ToDictionary(
                    exchangeGroup => exchangeGroup.Key,
                    exchangeGroup => exchangeGroup
                        .GroupBy(order => order.Product.Id)
                        .ToDictionary(
                            productGroup => productGroup.Key,
                            productGroup => productGroup.ToList()
                        )
                );

            var aggregatedPosition = new Position(position.Id, PositionStatus.PENDING, DateTimeOffset.UtcNow, position.PositionMandate, positionLegService);

            foreach (var item in groupedOrders)
            {
                foreach (var productGroup in item.Value)
                {
                    var ordersForProductOnExchange = productGroup.Value;

                    foreach (var order in ordersForProductOnExchange)
                    {
                        var fills = order.Fills.OrderBy(f => f.ExchangeTimestamp);
                        var exchange = order.Exchange;
                        var product = order.Product;

                        var existingLeg = aggregatedPosition.Legs.FirstOrDefault(l => l.Product.Id == product.Id && l.Exchange.Id == exchange.Id);
                        Option<PositionLeg> workLeg = None;
                        if (existingLeg is not null)
                        {
                            workLeg = Some(existingLeg with { AverageEntryPrice = 0, Quantity = 0, LastUpdated = DateTimeOffset.UtcNow });
                        }
                        else
                        {
                            var newLeg = new PositionLeg
                            {
                                AverageEntryPrice = 0,
                                Exchange = exchange,
                                Id = Guid.NewGuid(),
                                LastUpdated = DateTimeOffset.UtcNow,
                                Direction = order.Direction,
                                Product = product,
                                Quantity = 0
                            };
                            aggregatedPosition.AddLeg(newLeg);
                            workLeg = Some(newLeg);
                        }

                        var legRes = workLeg.Match(
                            Some: leg =>
                            {
                                var res = fills.Select(fill =>
                                {

                                    if (fill.Direction == Direction.LONG)
                                    {
                                        return aggregatedPosition.IncreaseLeg(leg.Id, fill.Quantity, fill.Price);
                                    }
                                    else if (fill.Direction == Direction.SHORT)
                                    {
                                        return aggregatedPosition.DecreaseLeg(leg.Id, fill.Quantity);
                                    }
                                    else
                                    {
                                        return Error.New("Invalid direction");
                                    }
                                }
                                ).ToList();

                                return res.All(r => r.IsSucc)
                                    ? FinSucc(unit)
                                    : Error.New(String.Join(", ", res.Where(r => r.IsFail).Select(f => f.Match(Fail: err => err.Message, Succ: _ => ""))));
                            },
                            None: () => Error.New("Leg not found")
                        );

                        if (legRes.IsFail)
                        {
                            return Error.New(legRes.Match(Fail: err => err.Message, Succ: _ => ""));
                        }
                    }
                }
            }

            return FinSucc(aggregatedPosition);
        }

        public Task<Fin<Unit>> UpdateAsync(Position position)
        {
            throw new NotImplementedException();
        }
    }
}
