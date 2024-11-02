using LanguageExt;

namespace domain.shared.services;

public interface IPriceService
{
    public Fin<decimal> GetNewAverageEntryPrice(decimal oldVolume, decimal additionalVolume, decimal oldAverageEntryPrice, decimal newEntryPrice);
}
