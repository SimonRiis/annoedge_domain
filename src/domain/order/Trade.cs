namespace domain.order;

public record Trade
{
    public required Guid Id { get; init; }
    public required Guid OrderId { get; init; }
    public required decimal Quantity { get; init; }
    public required decimal Price { get; init; }
    public required decimal TotalPriceInUSD { get; init; }
}
