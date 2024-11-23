using domain.algobook;
using domain.book;
using domain.exchangebook;
using domain.globalbook;
using LanguageExt;
using LanguageExt.Common;
using static LanguageExt.Prelude;

namespace domain.services.book;

public class BookService : IBookService
{
    private Dictionary<Guid, decimal> AggregateExposure(IEnumerable<BookEntry> entries)
    {
        return entries
            .GroupBy(entry => entry.Product.Symbol.Id)
            .Select(group => new
            {
                SymbolId = group.Key,
                AggregatedQuantity = group.Sum(entry => entry.Quantity)
            })
            .ToDictionary(x => x.SymbolId, x => x.AggregatedQuantity);
    }

    public Fin<bool> VerifyBooksForExchange(Guid exchangeId, IEnumerable<ExchangeBook> exchangeBooks, IEnumerable<AlgoBook> algoBooks)
    {
        // Set of AlgoBooks exchange IDs for filtering ExchangeBooks
        var algoExchangeIds = algoBooks
            .Where(x => x.Algo.Exchange.Id.Equals(exchangeId))
            .Select(x => x.Algo.Exchange.Id)
            .ToHashSet();

        var exchangeExposure = AggregateExposure(
            exchangeBooks
                .Where(x => algoExchangeIds.Contains(x.Exchange.Id))
                .SelectMany(exchangeBook => exchangeBook.Book.Entries)
        );

        var algoExposure = AggregateExposure(
            algoBooks
                .Where(x => x.Algo.Exchange.Id.Equals(exchangeId))
                .SelectMany(algoBook => algoBook.Book.Entries)
        );

        // Check for imbalances
        var discrepancies = exchangeExposure.Keys
            .Where(symbolId => exchangeExposure[symbolId] != algoExposure.GetValueOrDefault(symbolId))
            .Select(symbolId => $"IMBALANCE for Exchange id: {exchangeId} symbol id: {symbolId}")
            .ToList();

        return discrepancies.Count != 0
            ? Error.New(string.Join("\n", discrepancies))
            : FinSucc(true);
    }

    public Fin<bool> VerifyGlobalBook(GlobalBook globalBook, IEnumerable<ExchangeBook> exchangeBooks, IEnumerable<AlgoBook> algoBooks)
    {
        var globalExposure = AggregateExposure(globalBook.Book.Entries);
        var exchangeExposure = AggregateExposure(exchangeBooks.SelectMany(exchangeBook => exchangeBook.Book.Entries));

        // Check for imbalances
        var discrepancies = globalExposure.Keys
            .Where(symbolId => globalExposure[symbolId] != exchangeExposure.GetValueOrDefault(symbolId))
            .Select(symbolId => $"IMBALANCE for symbol id: {symbolId}")
            .ToList();

        return discrepancies.Count != 0
            ? Error.New(string.Join("\n", discrepancies))
            : FinSucc(true);
    }
}
