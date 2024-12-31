using domain.shared;
using domain.shared.enums;

namespace domain.services.order
{
    public record FillDTO
    {
        public required Direction Direction { get; init; }
        public required string Id { get; init; }
        public required string ExternalId { get; init; }
        public required string OrderId { get; init; }
        public required decimal Quantity { get; init; }
        public required decimal Price { get; init; }
        public required DateTimeOffset ExchangeTimestamp { get; init; }
    }
}
