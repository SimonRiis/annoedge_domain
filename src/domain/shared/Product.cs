using domain.shared;

namespace domain.shared;

public record Product
{
    public required Guid Id { get; init; }
    public required Symbol Symbol { get; init; }
    public required ProductInfo ProductInfo { get; init; }
}
