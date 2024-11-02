using System;
using LanguageExt;

namespace domain.book;

public interface IBookEntryService
{
    Fin<BookEntry> IncreaseVolume(BookEntry bookEntry, decimal additionalVolume, decimal newEntryPrice);
    Fin<BookEntry> DecreaseVolume(BookEntry bookEntry, decimal volumeReduction);  
}
