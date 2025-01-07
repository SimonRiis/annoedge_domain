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
    public Fin<Unit> UpdateStatusBasedOnIntents()
    {
        var hasUnmatchedIntents = Legs
        .Select(leg => leg.GetOpenUnfilledQuantity())
        .Any(qty => qty.Match(Succ: q => q > 0, Fail: _ => false));

        Status = hasUnmatchedIntents ? PositionStatus.PENDING : PositionStatus.OPEN;
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
            return Error.New("PositionOrderMap already exists for this order");

        _positionOrderMaps.Add(positionOrderMap);
        return FinSucc(unit);
    }

    public Fin<Unit> UpdateMandate(decimal stoplosspct, decimal pnlScaleTarget, decimal takeprofitpct, decimal currentlyAllocated)
    {
        if (stoplosspct >= 0)
            return Error.New("Stoploss must be a negative decimal less than 0");

        if (takeprofitpct <= 0)
            return Error.New("Take profit must be a non negative decimal bigger than 0");

        if (pnlScaleTarget <= 0)
            return Error.New("Pnl scale target must be a non negative decimal bigger than 0");

        if (currentlyAllocated < 0)
            return Error.New("Currently allocated must be a non negative decimal bigger than 0");

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
            return Error.New($"Position leg already exists for {leg.Product.Symbol} and {leg.Exchange} please increase leg instead");
        if (!leg.PositionId.Equals(Id))
            return Error.New($"Position leg belong to different position");
        _legs.Add(leg);
        return FinSucc(unit);
    }
    public Option<PositionLeg> GetLegById(Guid legId) => _legs.Find(l => l.Id == legId);

    private Fin<Unit> UpdateLeg(PositionLeg oldLeg, PositionLeg updatedLeg)
    {
        if (oldLeg == null)
            return Error.New("Leg not found");

        _legs[_legs.IndexOf(oldLeg)] = updatedLeg;
        return FinSucc(unit);
    }

    public Fin<Unit> IncreaseLeg(Guid legId, decimal additionalQuantity, decimal newEntryPrice)
    {
        var leg = GetLegById(legId);
        return leg.Match(
            Some: l => positionLegService.IncreaseQuantity(l, additionalQuantity, newEntryPrice)
                .Bind(updatedLeg => UpdateLeg(l, updatedLeg)),
            None: () => Error.New($"Position leg {legId} not found.")
        );
    }

    public Fin<Unit> DecreaseLeg(Guid legId, decimal decreaseQuantity)
    {
        var leg = GetLegById(legId);
        return leg.Match(
            Some: l => positionLegService.DecreaseQuantity(l, decreaseQuantity)
                .Bind(updatedLeg => UpdateLeg(l, updatedLeg)),
            None: () => Error.New($"Position leg {legId} not found.")
        );
    }

    public Fin<Unit> AddPositionLegIntent(PositionLeg positionLeg, Direction direction, decimal quantity, decimal priceTarget)
    {
        var intent = new PositionLegIntent
        {
            LegId = positionLeg.Id,
            PositionId = Id,
            CreatedAt = DateTimeOffset.UtcNow,
            Direction = direction,
            Id = Guid.NewGuid(),
            PriceTarget = priceTarget,
            Quantity = quantity,
            Status = shared.enums.Status.Pending,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        return positionLeg.AddIntent(intent).Bind(leg => UpdateLeg(positionLeg, leg));
    }
    public Fin<decimal> DecreasePositionLegIntent(PositionLeg positionLeg, decimal quantity)
    {
        var leg = GetLegById(positionLeg.Id);
        return leg.Match(
            Some: l => positionLegService.UpdateIntents(l, quantity)
                .Bind(updatedLeg =>
                {
                    var updateResult = UpdateLeg(l, updatedLeg.UpdatedLeg);
                    return updateResult.Match(
                        Succ: _ => FinSucc(updatedLeg.RemainingQuantity), // Return leftover quantity
                        Fail: e => Error.New($"Failed to update leg {l.Id}. Error: {e}")
                    );
                }),
            None: () => Error.New($"Position leg {positionLeg.Id} not found.")
        );
    }
}
