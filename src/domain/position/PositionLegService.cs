using domain.shared.services;
using LanguageExt;
using static LanguageExt.Prelude;
namespace domain.position;

public class PositionLegService(IPriceService priceService, IVolumeService volumeService) : IPositionLegService
{
    public Fin<PositionLeg> DecreaseVolume(PositionLeg leg, decimal volumeReduction)
    {
        // Keep the same AverageEntryPrice (since exiting doesn’t affect the entry price of the remaining position)
        return volumeService.DecreaseVolume(leg.Volume, volumeReduction).Bind(newVolume =>
        {
            var newLeg = leg with { Volume = newVolume, LastUpdated = DateTimeOffset.UtcNow };
            return FinSucc(newLeg);
        });
    }

    public Fin<PositionLeg> IncreaseVolume(PositionLeg leg, decimal additionalVolume, decimal newEntryPrice)
    {
        return priceService.GetNewAverageEntryPrice(leg.Volume, additionalVolume, leg.AverageEntryPrice, newEntryPrice).Bind(price =>
        {
            return volumeService.IncreaseVolume(leg.Volume, additionalVolume).Bind(newVolume =>
            {
                var newLeg = leg with { Volume = newVolume, AverageEntryPrice = price, LastUpdated = DateTimeOffset.UtcNow };
                return FinSucc(newLeg);
            });
        });
    }
}
