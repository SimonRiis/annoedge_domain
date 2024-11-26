using domain.shared;
using domain.shared.enums;

namespace domain.order;

public record Fill
{
    public required Direction Direction { get; init; }
    public required string Id { get; init; }
    public required string ExternalId { get; init; }
    public required string ExternalOrderId { get; init; }
    public required decimal Quantity { get; init; }
    public required decimal Price { get; init; }
    public required DateTimeOffset ExchangeTimestamp { get; init; }
    public required FillStatus Status { get; init; }

}
