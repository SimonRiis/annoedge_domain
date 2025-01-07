namespace domain.position;

public record PositionMandate
{
    public required Guid Id { get; init; } 
    public required decimal StoplossPct { get; init; } 
    public required decimal TakeProfitPct { get; init; } 
    public required decimal PnlScaleTarget { get; init; } 
    public required decimal MaxAllocated { get; init; } 
    public required decimal CurrentlyAllocated { get; init; } 
}
