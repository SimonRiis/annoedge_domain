using System;
using domain.book;

namespace domain.globalbook;

public record GlobalBook
{
    public required Book Book { get; init; }
    public required Guid Id { get; init; }
}
