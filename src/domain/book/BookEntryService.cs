using System;
using domain.shared.services;
using LanguageExt;
using LanguageExt.Common;
using static LanguageExt.Prelude;
namespace domain.book;

public class BookEntryService(IPriceService priceService, IQuantityService QuantityService) : IBookEntryService
{
    public Fin<BookEntry> DecreaseQuantity(BookEntry bookEntry, decimal QuantityReduction)
    {
        // Keep the same AverageEntryPrice (since exiting doesn’t affect the entry price of the remaining position)
        return QuantityService.DecreaseQuantity(bookEntry.Quantity, QuantityReduction).Bind(newQuantity =>
        {
            var newBookEntry = bookEntry with { Quantity = newQuantity, LastUpdated = DateTimeOffset.UtcNow };
            return FinSucc(newBookEntry);
        });
    }

    public Fin<BookEntry> IncreaseQuantity(BookEntry bookEntry, decimal additionalQuantity, decimal newEntryPrice)
    {
        return priceService.GetNewAverageEntryPrice(bookEntry.Quantity, additionalQuantity, bookEntry.AverageEntryPrice, newEntryPrice).Bind(price =>
        {
            return QuantityService.IncreaseQuantity(bookEntry.Quantity, additionalQuantity).Bind(newQuantity =>
            {
                var newBookEntry = bookEntry with { Quantity = newQuantity, AverageEntryPrice = price, LastUpdated = DateTimeOffset.UtcNow };
                return FinSucc(newBookEntry);
            });
        });
    }
}
