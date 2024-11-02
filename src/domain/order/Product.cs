using domain.shared;

namespace domain.order;

public record Product
{
    public required Guid ProductId { get; init; }
    public required Symbol Symbol { get; init; }
    public required ProductInfo ProductInfo { get; init; }
}
