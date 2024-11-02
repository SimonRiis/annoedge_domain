using domain.shared;
namespace domain.position;

public record PositionLeg
{
    public required Guid Id { get; init; } 
    public required decimal Volume { get; init; }
    public required decimal AverageEntryPrice { get; init; }
    public required DateTimeOffset LastUpdated { get; init; }
    public required Symbol Symbol { get; init; }
    public required PositionLegType LegType { get; init; }
    public required Exchange Exchange { get; init; }
}

