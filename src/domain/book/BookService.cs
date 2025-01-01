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

        private async Task<Fin<Unit>> UpdateOrAddBookEntries(Book book)
        {

            foreach (var bookEntry in book.Entries)
            {
                var bookEntryGetRes = await bookEntryRepository.GetByIdAsync(bookEntry.Id);

                var bookEntryRes = await bookEntryGetRes.Match(
                    Some: async (bookEntryGet) =>
                    {
                        var bookEntryRes = await bookEntryRepository.UpdateAsync(bookEntry);

                        if (bookEntryRes.IsFail)
                            return bookEntryRes;

                        return FinSucc(Unit.Default);
                    },
                    None: async () =>
                    {
                        var bookEntryRes = await bookEntryRepository.AddAsync(bookEntry);

                        if (bookEntryRes.IsFail)
                            return bookEntryRes;

                        return FinSucc(Unit.Default);
                    }
                );

                if (bookEntryRes.IsFail)
                    return bookEntryRes;
            }

            return FinSucc(Unit.Default);

            //return Task.WhenAll(book.Entries.Select(bookEntryRepository.UpdateAsync)).ContinueWith(task =>
            //{
            //    if (task.Result.All(r => r.IsSucc))
            //        return FinSucc(Unit.Default);
            //    else
            //        return FinFail<Unit>(string.Join(", ", task.Result.Where(r => r.IsFail).Select(r => r.ToString())));
            //});
        }

        public async Task<Fin<Unit>> UpdateAsync(Book book)
        {
            var bookRes = await bookRepository.UpdateAsync(book);

            if (bookRes.IsFail)
                return bookRes;

            var bookEntriesRes = await UpdateOrAddBookEntries(book);

            if (bookEntriesRes.IsFail)
                return bookEntriesRes;

            return FinSucc(Unit.Default);

        }
    }
}
