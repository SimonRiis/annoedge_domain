using domain.algobook;
using domain.exchangebook;
using domain.globalbook;
using LanguageExt;
using LanguageExt.Common;
using static LanguageExt.Prelude;

namespace domain.services.book
{
    public class BookVerifier(Guid globalBookId, IBookService bookService, IAlgoBookRepository algoBookRepository, IExchangeBookRepository exchangeBookRepository, IGlobalBookRepository globalBookRepository) : IBookVerifier
    {
        public async Task<Fin<Unit>> VerifyBooks()
        {
            // Fetch data from repositories
            var globalBookResult = await globalBookRepository.GetByIdAsync(globalBookId);
            var exchangeBooksResult = await exchangeBookRepository.GetAll();
            var algoBooksResult = await algoBookRepository.GetAll();

            // Handle global book result
            return globalBookResult.Match(
                Some: globalBook => VerifyExchangeBooks(globalBook, exchangeBooksResult, algoBooksResult),
                None: () => FinFail<Unit>($"Global book with id {globalBookId} not found")
            );
        }

        private Fin<Unit> VerifyExchangeBooks(GlobalBook globalBook, Fin<IEnumerable<ExchangeBook>> exchangeBooksResult, Fin<IEnumerable<AlgoBook>> algoBooksResult)
        {
            // Handle exchange books result
            return exchangeBooksResult.Match(
                Succ: exchangeBooks => VerifyAlgoBooks(globalBook, exchangeBooks.ToList(), algoBooksResult),
                Fail: FinFail<Unit>
            );
        }

        private Fin<Unit> VerifyAlgoBooks(GlobalBook globalBook, List<ExchangeBook> exchangeBooks, Fin<IEnumerable<AlgoBook>> algoBooksResult)
        {
            // Handle algo books result
            return algoBooksResult.Match(
                Succ: algoBooks => VerifyGlobalAndExchangeBooks(globalBook, exchangeBooks, algoBooks.ToList()),
                Fail: FinFail<Unit>
            );
        }

        private Fin<Unit> VerifyGlobalAndExchangeBooks(GlobalBook globalBook, List<ExchangeBook> exchangeBooks, List<AlgoBook> algoBooks)
        {
            // Verify global book
            var globalBookRes = bookService.VerifyGlobalBook(globalBook, exchangeBooks, algoBooks);

            return globalBookRes.Match(
                Succ: res => res
                    ? VerifyIndividualExchangeBooks(exchangeBooks, algoBooks)
                    : FinFail<Unit>("Global book verification failed"),
                Fail: FinFail<Unit>
            );
        }

        private Fin<Unit> VerifyIndividualExchangeBooks(List<ExchangeBook> exchangeBooks, List<AlgoBook> algoBooks)
        {
            // Verify individual exchange books
            var exchangeBookResults = exchangeBooks
                .Select(exchangeBook =>
                    bookService.VerifyBooksForExchange(exchangeBook.Exchange.Id, exchangeBooks, algoBooks)
                )
                .ToList(); // Materialize results

            // Check all results
            return exchangeBookResults.All(r => r.IsSucc)
                ? FinSucc(Unit.Default)
                : FinFail<Unit>(string.Join(", ", exchangeBookResults.Fails().Select(r => r.Message)));
        }

    }
}
