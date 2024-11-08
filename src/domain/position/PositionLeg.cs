using domain.shared;
namespace domain.position;

public record PositionLeg
{
    public required Guid Id { get; init; } 
    public decimal Quantity { get; init; }
    public required decimal AverageEntryPrice { get; init; }
    public required DateTimeOffset LastUpdated { get; init; }
    public required Product Product { get; init; }
    public required PositionLegType LegType { get; init; }
    public required Exchange Exchange { get; init; }
}

