using System;
using domain.exchange;

namespace domain.shared;

public record Algo
{
    public required string Name { get; init; }
    public required Guid Id { get; init; }
    public required Exchange Exchange { get; init; }
    public override string ToString() => $"{Name} - ({Id}) Exchange: {Exchange}";
}
