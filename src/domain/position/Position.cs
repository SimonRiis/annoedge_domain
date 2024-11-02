using LanguageExt;
using LanguageExt.Common;
using static LanguageExt.Prelude;
namespace domain.position;

public class Position(Guid id, PositionStatus status, DateTimeOffset lastUpdated, IPositionLegService positionLegService) : IEquatable<Position>
{
    public Guid Id { get; init; } = id;
    public PositionStatus Status { get; private set; } = status;
    public DateTimeOffset LastUpdated { get; private set; } = lastUpdated;
    private readonly List<PositionLeg> _legs = [];
    public IReadOnlyCollection<PositionLeg> Legs => _legs.AsReadOnly();

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

    // Add a leg to the position
    public Fin<Unit> AddLeg(PositionLeg leg)
    {
        var existingLeg = _legs.FirstOrDefault(l => l.Symbol == leg.Symbol && l.Exchange == leg.Exchange);
        if (existingLeg != null)
            return Error.New($"Position leg already exists for {leg.Symbol} and {leg.Exchange} please increase leg instead");
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

    public Fin<Unit> IncreaseLeg(Guid legId, decimal additionalVolume, decimal newEntryPrice)
    {
        var leg = GetLegById(legId);
        return leg.Match(
            Some: l => positionLegService.IncreaseVolume(l, additionalVolume, newEntryPrice)
                .Bind(updatedLeg => UpdateLeg(l, updatedLeg)),
            None: () => Error.New($"Position leg {legId} not found.")
        );
    }

    public Fin<Unit> DecreaseLeg(Guid legId, decimal decreaseVolume)
    {
        var leg = GetLegById(legId);
        return leg.Match(
            Some: l => positionLegService.DecreaseVolume(l, decreaseVolume)
                .Bind(updatedLeg => UpdateLeg(l, updatedLeg)),
            None: () => Error.New($"Position leg {legId} not found.")
        );
    }
}
