namespace domain.services.book
{
    public record GlobalBookDTO
    {
        public required BookDTO Book { get; init; }
        public required Guid Id { get; init; }
    }
}
