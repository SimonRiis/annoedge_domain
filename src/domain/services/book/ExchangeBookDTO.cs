using domain.shared;

namespace domain.services.book
{
    public record ExchangeBookDTO
    {
        public required string Name { get; init; }
        public required BookDTO Book { get; init; }
        public required Guid Id { get; init; }
        public required Exchange Exchange { get; init; }
    }
}
