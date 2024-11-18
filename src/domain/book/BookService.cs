using domain.position;
using domain.shared;
using LanguageExt;
using static LanguageExt.Prelude;

namespace domain.book
{
    public class BookService(IBookRepository bookRepository, IBookEntryRepository bookEntryRepository) : IBookService
    {
        public Task<Option<Book>> GetForId(Guid id)
        {
            return bookRepository.GetByIdAsync(id);
        }

        private Task<Fin<Unit>> UpdateBookEntries(Book book)
        {
            return Task.WhenAll(book.Entries.Select(bookEntryRepository.UpdateAsync)).ContinueWith(task =>
            {
                if (task.Result.All(r => r.IsSucc))
                    return FinSucc(Unit.Default);
                else
                    return FinFail<Unit>(string.Join(", ", task.Result.Where(r => r.IsFail).Select(r => r.ToString())));
            });
        }

        public async Task<Fin<Unit>> UpdateAsync(Book book)
        {
            var bookRes = bookRepository.UpdateAsync(book);
            var bookEntriesRes = UpdateBookEntries(book);
            var tasks = new List<Task<Fin<Unit>>> { bookRes, bookEntriesRes };
            var results = await Task.WhenAll(tasks);

            return results.FlattenResult();
        }
    }
}
