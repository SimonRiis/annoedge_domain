namespace domain.exchange;
public record Exchange
{
    public required string Name { get; init; }
    public required Guid Id { get; init; }
    public required Guid DefaultBookId { get; init; }
    public override string ToString() => $"{Name} ({Id})";
}
