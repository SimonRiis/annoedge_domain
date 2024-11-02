namespace domain.shared;
public record Exchange
{
    public required string Name { get; init; }
    public required Guid Id { get; init; }
    public override string ToString() => $"{Name} ({Id})";
}
