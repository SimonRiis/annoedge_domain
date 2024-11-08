using LanguageExt;

namespace domain.shared.services;

public interface IPriceService
{
    public Fin<decimal> GetNewAverageEntryPrice(decimal oldQuantity, decimal additionalQuantity, decimal oldAverageEntryPrice, decimal newEntryPrice);
}
