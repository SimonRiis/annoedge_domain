using LanguageExt;
using LanguageExt.Common;
using static LanguageExt.Prelude;

namespace domain.shared.services;

public class QuantityService : IQuantityService
{
    public Fin<decimal> DecreaseQuantity(decimal currentQuantity, decimal QuantityReduction)
    {
        if (QuantityReduction <= 0)
            return Error.New("Invalid quantity");

        if (QuantityReduction > currentQuantity)
            return Error.New("Decrease Quantity cannot exceed current position Quantity.");

        // Calculate the new Quantity after decreasing
        var newQuantity = currentQuantity - QuantityReduction;
        return FinSucc(newQuantity);
    }

    public Fin<decimal> IncreaseQuantity(decimal currentQuantity, decimal additionalQuantity)
    {
        if (additionalQuantity <= 0)
            return Error.New("Invalid quantity");
        var newQuantity = currentQuantity + additionalQuantity;
        return FinSucc(newQuantity);
    }
}
