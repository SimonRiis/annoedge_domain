using System;
using domain.book;
using domain.shared;

namespace domain.exchangebook;

public record ExchangeBook
{
    public required string Name {get; init;}
    public required Book Book {get; init;}
    public required Guid Id {get; init;}
    public required Exchange Exchange {get; init;}
}
