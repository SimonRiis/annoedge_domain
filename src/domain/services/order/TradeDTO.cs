using domain.shared;
using domain.shared.enums;

namespace domain.services.order
{
    public record TradeDTO
    {
        public required Direction Direction { get; init; }
        public required ExchangeMapId Id { get; init; }
        public required ExchangeMapId OrderId { get; init; }
        public required decimal Quantity { get; init; }
        public required decimal Price { get; init; }
        public required decimal TotalPriceInUSD { get; init; }
        public required DateTimeOffset ExchangeTimestamp { get; init; }
    }
}
