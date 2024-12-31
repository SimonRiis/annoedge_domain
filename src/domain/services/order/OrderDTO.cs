using domain.exchange;
using domain.product;
using domain.shared.enums;

namespace domain.services.order
{
    public record OrderDTO
    {
        public required Direction Direction { get; init; }
        public required string Id { get; init; }
        public required string ExternalId { get; init; }
        public required Product Product { get; init; }
        public required decimal Quantity { get; init; }
        public required decimal Price { get; init; }
        public required Exchange Exchange { get; init; }
        public required Status Status { get; init; }
        public required DateTimeOffset LastUpdated { get; init; }
        public required DateTimeOffset ExchangeTimestamp { get; init; }
        public required List<TradeDTO> Trades { get; init; }
        public required List<FillDTO> Fills { get; init; }
    }
}
