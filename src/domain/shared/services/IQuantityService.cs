using LanguageExt;

namespace domain.shared.services;

public interface IQuantityService
{
    public Fin<decimal> IncreaseQuantity(decimal currentQuantity, decimal additionalQuantity);
    public Fin<decimal> DecreaseQuantity(decimal currentQuantity, decimal QuantityReduction);
}
