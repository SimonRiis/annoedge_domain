using LanguageExt;
using LanguageExt.Common;
using static LanguageExt.Prelude;
namespace domain.shared.services;

public class PriceService : IPriceService
{
    public Fin<decimal> GetNewAverageEntryPrice(decimal oldQuantity, decimal additionalQuantity, decimal oldAverageEntryPrice, decimal newEntryPrice)
    {
        if (additionalQuantity <= 0)
            return Error.New("Additional Quantity cannot be zero.");

        if (newEntryPrice <= 0)
            return Error.New("New EntryPrice cannot be zero.");

        return FinSucc(((oldQuantity * oldAverageEntryPrice) + (additionalQuantity * newEntryPrice)) / (oldQuantity + additionalQuantity));
    }
}
