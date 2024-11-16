using domain.shared;
using domain.shared.enums;
namespace domain.position;

public record PositionLeg
{
    public required Guid Id { get; init; }
    public required Guid BookId { get; init; }
    public required decimal Quantity { get; init; }
    public required decimal AverageEntryPrice { get; init; }
    public required DateTimeOffset LastUpdated { get; init; }
    public required Product Product { get; init; }
    public required Direction Direction { get; init; }
    public required Exchange Exchange { get; init; }
}

