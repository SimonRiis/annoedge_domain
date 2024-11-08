using domain.shared.services;
using LanguageExt;
using static LanguageExt.Prelude;
namespace domain.position;

public class PositionLegService(IPriceService priceService, IQuantityService QuantityService) : IPositionLegService
{
    public Fin<PositionLeg> DecreaseQuantity(PositionLeg leg, decimal QuantityReduction)
    {
        // Keep the same AverageEntryPrice (since exiting doesn’t affect the entry price of the remaining position)
        return QuantityService.DecreaseQuantity(leg.Quantity, QuantityReduction).Bind(newQuantity =>
        {
            var newLeg = leg with { Quantity = newQuantity, LastUpdated = DateTimeOffset.UtcNow };
            return FinSucc(newLeg);
        });
    }

    public Fin<PositionLeg> IncreaseQuantity(PositionLeg leg, decimal additionalQuantity, decimal newEntryPrice)
    {
        return priceService.GetNewAverageEntryPrice(leg.Quantity, additionalQuantity, leg.AverageEntryPrice, newEntryPrice).Bind(price =>
        {
            return QuantityService.IncreaseQuantity(leg.Quantity, additionalQuantity).Bind(newQuantity =>
            {
                var newLeg = leg with { Quantity = newQuantity, AverageEntryPrice = price, LastUpdated = DateTimeOffset.UtcNow };
                return FinSucc(newLeg);
            });
        });
    }
}
