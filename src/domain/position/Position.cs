using domain.shared.enums;
using LanguageExt;
using LanguageExt.Common;
using static LanguageExt.Prelude;
namespace domain.position;

public class Position(Guid id, PositionStatus status, DateTimeOffset lastUpdated, PositionMandate positionMandate, IPositionLegService positionLegService) : IEquatable<Position>
{
    public Guid Id { get; init; } = id;
    public PositionMandate PositionMandate { get; private set; } = positionMandate;
    public PositionStatus Status { get; private set; } = status;
    public DateTimeOffset LastUpdated { get; private set; } = lastUpdated;
    private readonly List<PositionLeg> _legs = [];
    public IReadOnlyCollection<PositionLeg> Legs => _legs.AsReadOnly();

    private readonly List<PositionPnLEvent> _pnlEvents = [];
    public IReadOnlyCollection<PositionPnLEvent> PnlEvents => _pnlEvents.AsReadOnly();

    private readonly List<PositionEvent> _events = [];
    public IReadOnlyCollection<PositionEvent> Events => _events.AsReadOnly();

    private readonly List<PositionOrderMap> _positionOrderMaps = [];
    public IReadOnlyCollection<PositionOrderMap> PositionOrderMaps => _positionOrderMaps.AsReadOnly();

    public bool Equals(Position? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;

        return Id == other.Id;
    }

    public Fin<Unit> SetPositionToClosing()
    {
        //TODO: should we check / validate something here?
        //if (Legs.Any(x => x.Quantity > 0))
        //{
        //    return Error.New($"Position {Id} position could not be closed, still open quantity on legs");
        //}

        Status = PositionStatus.CLOSING;
        return FinSucc(unit);
    }

    private Fin<Unit> ClosePosition()
    {
        if (Legs.Any(x => x.Quantity > 0))
        {
            return Error.New($"Position {Id} position could not be closed, still open quantity on legs");
        }

        Status = PositionStatus.CLOSED;
        return FinSucc(unit);
    }

    public Fin<Unit> UpdateStatusBasedOnIntents()
    {
        var hasUnmatchedIntents = Legs
        .Select(leg => leg.GetOpenUnfilledQuantity())
        .Any(qty => qty.Match(Succ: q => q > 0, Fail: _ => false));

        if(hasUnmatchedIntents && Status != PositionStatus.CLOSING)
        {
            Status = PositionStatus.PENDING; // If there are unmatched intents, set the position to pending, however if the position is already closing, do not change the status
            //that that the position can't be scaled no matter the status. 
            //This is to prevent the position from being scaled while it is closing, ie waiting for intents to be matched
        }
        else if(!hasUnmatchedIntents && Status == PositionStatus.CLOSING)
        {
            return ClosePosition(); // Close the position if all intents are matched
        }
        else if(Status != PositionStatus.CLOSING && Status != PositionStatus.CLOSED)
        {
            Status = PositionStatus.OPEN; // If there are no unmatched intents, and we are no closing set the position to open
        }
        return FinSucc(Unit.Default);
    }

    // Override Equals(object obj) for general equality comparison
    public override bool Equals(object? obj) =>
        obj is Position other && Equals(other);

    // Override GetHashCode to match the equality logic in Equals
    public override int GetHashCode()
    {
        return HashCode.Combine(Id);
    }

    public Fin<Unit> AddEvent(PositionEventType eventType, PositionStatus oldStatus, PositionStatus newStatus, string eventData)
    {
        var positionEvent = new PositionEvent
        {
            Id = Guid.NewGuid(),
            PositionId = Id,
            EventType = eventType,
            OldStatus = oldStatus,
            NewStatus = newStatus,
            EventData = eventData,
            CreatedAt = DateTimeOffset.UtcNow
        };

        _events.Add(positionEvent);
        return FinSucc(unit);
    }

    public Fin<Unit> AddPnLEvent(decimal pnl, decimal pnlPct)
    {
        var pnlEvent = new PositionPnLEvent
        {
            Id = Guid.NewGuid(),
            PositionId = Id,
            Pnl = pnl,
            PnlPct = pnlPct,
            Timestamp = DateTimeOffset.UtcNow
        };

        _pnlEvents.Add(pnlEvent);
        return FinSucc(unit);
    }

    public Fin<Unit> AddPositionOrderMap(PositionOrderMap positionOrderMap)
    {
        if (_positionOrderMaps.Any(pom => pom.OrderId == positionOrderMap.OrderId))
            return Error.New($"Position {Id} PositionOrderMap already exists for this order");

        _positionOrderMaps.Add(positionOrderMap);
        return FinSucc(unit);
    }

    public Fin<Unit> UpdateMandate(decimal stoplosspct, decimal pnlScaleTarget, decimal takeprofitpct, decimal currentlyAllocated)
    {
        if (stoplosspct <= 0)
            return Error.New($"Position {Id} Stoploss must be a non negative decimal bigger than 0");

        if (takeprofitpct <= 0)
            return Error.New($"Position {Id} Take profit must be a non negative decimal bigger than 0");

        if (pnlScaleTarget <= 0)
            return Error.New($"Position {Id} Pnl scale target must be a non negative decimal bigger than 0");

        if (currentlyAllocated < 0)
            return Error.New($"Position {Id} Currently allocated must be a non negative decimal bigger than 0");

        PositionMandate = PositionMandate with
        {
            StoplossPct = stoplosspct,
            TakeProfitPct = takeprofitpct,
            PnlScaleTarget = pnlScaleTarget,
            CurrentlyAllocated = currentlyAllocated
        };

        return FinSucc(unit);
    }

    // Add a leg to the position
    public Fin<Unit> AddLeg(PositionLeg leg)
    {
        var existingLeg = _legs.FirstOrDefault(l => l.Product.Id == leg.Product.Id && l.Exchange == leg.Exchange);
        if (existingLeg != null)
            return Error.New($"Position {Id} leg already exists for {leg.Product.Symbol} and {leg.Exchange} please increase leg instead");
        if (!leg.PositionId.Equals(Id))
            return Error.New($"Position {Id} leg belong to different position");
        _legs.Add(leg);
        return FinSucc(unit);
    }
    public Option<PositionLeg> GetLegById(Guid legId) => _legs.Find(l => l.Id == legId);

    private Fin<Unit> UpdateLeg(Guid legId, PositionLeg updatedLeg)
    {
        var oldLeg = GetLegById(legId);
       return oldLeg.Match(
            Some: l =>
            {
                try
                {
                    _legs[_legs.IndexOf(l)] = updatedLeg;
                    return FinSucc(unit);
                }
                catch (Exception e)
                {
                    return Error.New($"Position {Id} Failed to update leg {legId}. Error: {e}");
                }
            },
            None: () => Error.New($"Position {Id} Leg not found")
        );
    }

    public Fin<Unit> IncreaseLeg(Guid legId, decimal additionalQuantity, decimal newEntryPrice)
    {
        var leg = GetLegById(legId);
        return leg.Match(
            Some: l => positionLegService.IncreaseQuantity(l, additionalQuantity, newEntryPrice)
                .Bind(updatedLeg => UpdateLeg(l.Id, updatedLeg)),
            None: () => Error.New($"Position {Id} Position leg {legId} not found.")
        );
    }

    public Fin<Unit> DecreaseLeg(Guid legId, decimal decreaseQuantity)
    {
        var leg = GetLegById(legId);
        return leg.Match(
            Some: l => positionLegService.DecreaseQuantity(l, decreaseQuantity)
                .Bind(updatedLeg => UpdateLeg(l.Id, updatedLeg)),
            None: () => Error.New($"Position {Id} Position leg {legId} not found.")
        );
    }

    public Fin<Unit> AddPositionLegIntent(Guid legId, Direction direction, decimal quantity, decimal priceTarget)
    {
        var positionLeg = GetLegById(legId);
        return positionLeg.Match(
            Some: l => {
                var intent = new PositionLegIntent
                {
                    LegId = legId,
                    PositionId = Id,
                    CreatedAt = DateTimeOffset.UtcNow,
                    Direction = direction,
                    Id = Guid.NewGuid(),
                    PriceTarget = priceTarget,
                    Quantity = quantity,
                    Status = shared.enums.Status.PENDING,
                    UpdatedAt = DateTimeOffset.UtcNow
                };

                return l.AddIntent(intent).Bind(leg => UpdateLeg(legId, leg)); },
            None: () => Error.New($"Position {Id} Position leg {legId} not found.")
        );
    }
    public Fin<decimal> DecreasePositionLegIntent(Guid legId, decimal quantity)
    {
        var leg = GetLegById(legId);
        return leg.Match(
            Some: l => positionLegService.UpdateIntents(l, quantity)
                .Bind(updatedLeg =>
                {
                    var updateResult = UpdateLeg(l.Id, updatedLeg.UpdatedLeg);
                    return updateResult.Match(
                        Succ: _ => FinSucc(updatedLeg.RemainingQuantity), // Return leftover quantity
                        Fail: e => Error.New($"Position {Id} Failed to update leg {l.Id}. Error: {e}")
                    );
                }),
            None: () => Error.New($"Position {Id} Position leg {legId} not found.")
        );
    }
}
