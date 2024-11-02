namespace domain.shared;

public record Symbol
{
    public required string Name { get; init; }
    public required Guid Id { get; init; }
    //public required Exchange Exchange { get; init; }
    public override string ToString() => $"{Name} ({Id})";
}

//TODO make map between exchange symbol names and internal
