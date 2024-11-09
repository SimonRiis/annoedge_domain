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

    private readonly List<PositionOrderMap> _positionOrderMaps = [];
    public IReadOnlyCollection<PositionOrderMap> PositionOrderMaps => _positionOrderMaps.AsReadOnly();

    public bool Equals(Position? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;

        return Id == other.Id;
    }

    // Override Equals(object obj) for general equality comparison
    public override bool Equals(object? obj) =>
        obj is Position other && Equals(other);

    // Override GetHashCode to match the equality logic in Equals
    public override int GetHashCode()
    {
        return HashCode.Combine(Id);
    }

    public Fin<Unit> AddPositionOrderMap(PositionOrderMap positionOrderMap)
    {
        if (_positionOrderMaps.Any(pom => pom.OrderId == positionOrderMap.OrderId))
            return Error.New("PositionOrderMap already exists for this order");

        _positionOrderMaps.Add(positionOrderMap);
        return FinSucc(unit);
    }

    public Fin<Unit> UpdateMandate(decimal stoplosspct, decimal takeprofitpct)
    {
        if (stoplosspct >= 0)
            return Error.New("Stoploss must be a negative decimal less than 0");

        if (takeprofitpct <= 0)
            return Error.New("Take profit must be a non negative decimal bigger than 0");

        PositionMandate = PositionMandate with
        {
            StoplossPct = stoplosspct,
            TakeProfitPct = takeprofitpct
        };

        return FinSucc(unit);
    }

    // Add a leg to the position
    public Fin<Unit> AddLeg(PositionLeg leg)
    {
        var existingLeg = _legs.FirstOrDefault(l => l.Product.Id == leg.Product.Id && l.Exchange == leg.Exchange);
        if (existingLeg != null)
            return Error.New($"Position leg already exists for {leg.Product.Symbol} and {leg.Exchange} please increase leg instead");
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
}
