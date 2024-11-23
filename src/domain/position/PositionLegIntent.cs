using domain.shared.enums;

namespace domain.position
{
    public record PositionLegIntent
    {
        public required Guid Id { get; init; }
        public required decimal Quantity { get; init; }
        public required decimal PriceTarget { get; init; }
        public required Direction Direction { get; init; }
        public required DateTimeOffset CreatedAt { get; init; }
        public required DateTimeOffset UpdatedAt { get; init; }
        public required Status Status { get; init; }
    }
}
