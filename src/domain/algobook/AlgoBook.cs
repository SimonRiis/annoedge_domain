using System;
using domain.book;
using domain.shared;

namespace domain.algobook;

public record AlgoBook
{
    public required Book Book {get; init;}
    public required Guid Id {get; init;}
    public required Algo Algo {get; init;}
}
