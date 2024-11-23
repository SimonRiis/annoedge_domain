using domain.algobook;
using domain.exchangebook;
using domain.globalbook;
using LanguageExt;

namespace domain.services.book;

public interface IBookService
{
    public Fin<bool> VerifyGlobalBook(GlobalBook globalBook, IEnumerable<ExchangeBook> exchangeBooks, IEnumerable<AlgoBook> algoBooks);
    public Fin<bool> VerifyBooksForExchange(Guid exchangeId, IEnumerable<ExchangeBook> exchangeBooks, IEnumerable<AlgoBook> algoBooks);
}
