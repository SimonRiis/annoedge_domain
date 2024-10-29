using System;
using System.ComponentModel;
using System.Data.Common;
using System.Dynamic;

namespace domain.position;
public class Exchange(string name, Guid id) : IEquatable<Exchange>
{
    public string Name { get; init; } = name;
    public Guid Id { get; init; } = id;

    public bool Equals(Exchange? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;

        return Name == other.Name && Id == other.Id;
    }

    // Override Equals(object obj) for general equality comparison
    public override bool Equals(object? obj) =>
        obj is Exchange other && Equals(other);

    // Override GetHashCode to match the equality logic in Equals
    public override int GetHashCode()
    {
        return HashCode.Combine(Name, Id);
    }

    public override string ToString() => $"{Name} ({Id})";
}
