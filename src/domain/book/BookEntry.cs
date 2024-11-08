using System;
using domain.shared;

namespace domain.book;

public record BookEntry
{
    public required Guid Id { get; init; }
    public required Product Product { get; init; }
    public decimal Quantity { get; init; }
    public required decimal AverageEntryPrice { get; init; }
    public required DateTimeOffset LastUpdated { get; init; }
}
