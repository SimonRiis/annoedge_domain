using System;

namespace domain.position;

public class Symbol(string name, Guid id, Exchange exchange) : IEquatable<Symbol>
{
    public string Name { get; init; } = name;
    public Guid Id { get; init; } = id;
    public Exchange Exchange { get; init; } = exchange;

    // Implement IEquatable<Symbol> for type-safe equality comparison
    public bool Equals(Symbol? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;

        // Define equality based on Id and Exchange.Id
        return Id == other.Id && Exchange.Id == other.Exchange.Id;
    }

    // Override Equals(object obj) for general equality comparison
    public override bool Equals(object? obj) =>
        obj is Symbol other && Equals(other);

    // Override GetHashCode to match the equality logic in Equals
    public override int GetHashCode()
    {
        return HashCode.Combine(Id, Exchange.Id);
    }

    public override string ToString() => $"{Name} ({Id}) on {Exchange.Name}";
}
