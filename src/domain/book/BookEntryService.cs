using System;
using domain.shared.services;
using LanguageExt;
using LanguageExt.Common;
using static LanguageExt.Prelude;
namespace domain.book;

public class BookEntryService(IPriceService priceService, IVolumeService volumeService) : IBookEntryService
{
    public Fin<BookEntry> DecreaseVolume(BookEntry bookEntry, decimal volumeReduction)
    {
        // Keep the same AverageEntryPrice (since exiting doesn’t affect the entry price of the remaining position)
        return volumeService.DecreaseVolume(bookEntry.Volume, volumeReduction).Bind(newVolume =>
        {
            var newBookEntry = bookEntry with { Volume = newVolume, LastUpdated = DateTimeOffset.UtcNow };
            return FinSucc(newBookEntry);
        });
    }

    public Fin<BookEntry> IncreaseVolume(BookEntry bookEntry, decimal additionalVolume, decimal newEntryPrice)
    {
        return priceService.GetNewAverageEntryPrice(bookEntry.Volume, additionalVolume, bookEntry.AverageEntryPrice, newEntryPrice).Bind(price =>
        {
            return volumeService.IncreaseVolume(bookEntry.Volume, additionalVolume).Bind(newVolume =>
            {
                var newBookEntry = bookEntry with { Volume = newVolume, AverageEntryPrice = price, LastUpdated = DateTimeOffset.UtcNow };
                return FinSucc(newBookEntry);
            });
        });
    }
}
