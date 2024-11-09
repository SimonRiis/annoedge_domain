using domain.shared;

namespace domain.services.book
{
    public record AlgoBookDTO
    {
        public required BookDTO Book { get; init; }
        public required Guid Id { get; init; }
        public required Algo Algo { get; init; }
    }
}
