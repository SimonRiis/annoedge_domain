using System;
using LanguageExt;

namespace domain.book;

public interface IBookEntryService
{
    Fin<BookEntry> IncreaseQuantity(BookEntry bookEntry, decimal additionalQuantity, decimal newEntryPrice);
    Fin<BookEntry> DecreaseQuantity(BookEntry bookEntry, decimal QuantityReduction);  
}
