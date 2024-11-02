using System;
using domain.shared;

namespace domain.book;

public record BookEntry
{
    public required Guid Id { get; init; }
    public required Symbol Symbol { get; init; }
    public required decimal Volume { get; init; }
    public required decimal AverageEntryPrice { get; init; }
    public required DateTimeOffset LastUpdated { get; init; }
}
