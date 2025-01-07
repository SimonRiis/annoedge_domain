using domain.shared.enums;
using domain.shared.services;
using LanguageExt;
using Microsoft.Extensions.Logging;
using static LanguageExt.Prelude;
namespace domain.position;

public class PositionLegService(ILogger<PositionLegService> logger, IPriceService priceService, IQuantityService QuantityService) : IPositionLegService
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

    public Fin<UpdateIntentsResult> UpdateIntents(PositionLeg leg, decimal filledQuantity)
    {
        var unfilledPositionIntents = leg.PositionLegIntents
            .Where(i => i.Status != Status.FILLED && i.Status != Status.CANCELED)
            .ToList();

        return ProcessIntentsRecursively(leg, unfilledPositionIntents, filledQuantity);
    }

    private Fin<UpdateIntentsResult> ProcessIntentsRecursively(
        PositionLeg leg,
        List<PositionLegIntent> intents,
        decimal remainingQuantity)
    {
        // Base case: no intents left or no remaining quantity to process
        if (!intents.Any() || remainingQuantity <= 0)
        {
            return FinSucc(new UpdateIntentsResult(leg, remainingQuantity));
        }

        // Extract the first intent
        var currentIntent = intents.First();
        var quantityToDecrease = Math.Min(currentIntent.Quantity, remainingQuantity);

        // Attempt to decrease the intent quantity
        var updatedLeg = leg.DecreaseIntentQuantity(currentIntent, quantityToDecrease);

        return updatedLeg.Match(
            Succ: updated =>
            {
                // Recursive call with updated state
                var remainingIntents = intents.Skip(1).ToList();
                return ProcessIntentsRecursively(updated, remainingIntents, remainingQuantity - quantityToDecrease);
            },
            Fail: e =>
            {
                logger.LogError("Failed to decrease intent quantity: {Message}", e.Message);
                // Skip the current intent if decreasing fails and proceed with the rest
                var remainingIntents = intents.Skip(1).ToList();
                return ProcessIntentsRecursively(leg, remainingIntents, remainingQuantity);
            }
        );
    }

}
