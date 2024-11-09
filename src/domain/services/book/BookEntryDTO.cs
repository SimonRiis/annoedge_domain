using domain.shared;

namespace domain.services.book
{
    public record BookEntryDTO
    {
        public required Guid Id { get; init; }
        public required Product Product { get; init; }
        public decimal Quantity { get; init; }
        public required decimal AverageEntryPrice { get; init; }
        public required DateTimeOffset LastUpdated { get; init; }
    }
}
