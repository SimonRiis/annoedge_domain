using LanguageExt;
using LanguageExt.Common;
using static LanguageExt.Prelude;
namespace domain.shared.services;

public class PriceService : IPriceService
{
    public Fin<decimal> GetNewAverageEntryPrice(decimal oldVolume, decimal additionalVolume, decimal oldAverageEntryPrice, decimal newEntryPrice)
    {
        if (additionalVolume <= 0)
            return Error.New("Additional volume cannot be zero.");

        if (newEntryPrice <= 0)
            return Error.New("New EntryPrice cannot be zero.");

        return FinSucc(((oldVolume * oldAverageEntryPrice) + (additionalVolume * newEntryPrice)) / (oldVolume + additionalVolume));
    }
}
