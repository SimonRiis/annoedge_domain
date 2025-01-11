using LanguageExt;
using LanguageExt.Common;
using static LanguageExt.Prelude;
namespace domain.book;

public class Book(IBookEntryService bookEntryService)
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
    private readonly List<BookEntry> _entries = [];
    public IReadOnlyCollection<BookEntry> Entries => _entries.AsReadOnly();

    public Option<BookEntry> GetBookEntryByProductId(Guid productId) => _entries.Find(b => b.Product.Id == productId);
    public Option<BookEntry> GetBookEntryById(Guid bookEntryId) => _entries.Find(b => b.Id == bookEntryId);
    public Fin<Unit> AddBookEntry(BookEntry bookEntry)
    {
        var entry = GetBookEntryById(bookEntry.Id);
        return entry.Match(
            Some: l => Error.New($"Already contains a bookentry for id: {bookEntry.Id}"),
            None: () =>
            {
                _entries.Add(bookEntry);
                return FinSucc(unit);
            });
    }

    private Fin<Unit> UpdateBookEntry(Guid oldBookEntryId, BookEntry updatedBookEntry)
    {
        var bookEntry = GetBookEntryById(oldBookEntryId);
        return bookEntry.Match(
           Some: b =>
           {
               _entries[_entries.IndexOf(b)] = updatedBookEntry;
               return FinSucc(unit);
           },
           None: () => Error.New($"BookEntry for id {oldBookEntryId} not found.")
       );
    }
    public Fin<Unit> IncreaseBookEntryQuantity(Guid bookEntryId, decimal additionalQuantity, decimal newEntryPrice)
    {
        var bookEntry = GetBookEntryById(bookEntryId);
        return bookEntry.Match(
            Some: b => bookEntryService.IncreaseQuantity(b, additionalQuantity, newEntryPrice)
                .Bind(updatedLeg => UpdateBookEntry(b.Id, updatedLeg)),
            None: () => Error.New($"BookEntry for id {bookEntryId} not found.")
        );
    }

    public Fin<Unit> DecreaseBookEntryQuantity(Guid bookEntryId, decimal decreaseQuantity)
    {
        var bookEntry = GetBookEntryById(bookEntryId);
        return bookEntry.Match(
            Some: b => bookEntryService.DecreaseQuantity(b, decreaseQuantity)
                .Bind(updatedLeg => UpdateBookEntry(b.Id, updatedLeg)),
            None: () => Error.New($"BookEntry for id {bookEntryId} not found.")
        );
    }
}
