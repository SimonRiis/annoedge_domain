using LanguageExt;
using LanguageExt.Common;
using static LanguageExt.Prelude;

namespace domain.shared.services;

public class VolumeService : IVolumeService
{
    public Fin<decimal> DecreaseVolume(decimal currentVolume, decimal volumeReduction)
    {
        if (volumeReduction <= 0)
            return Error.New("Decrease volume cannot be zero.");

        if (volumeReduction > currentVolume)
            return Error.New("Decrease volume cannot exceed current position volume.");

        // Calculate the new volume after decreasing
        var newVolume = currentVolume - volumeReduction;
        return FinSucc(newVolume);
    }

    public Fin<decimal> IncreaseVolume(decimal currentVolume, decimal additionalVolume)
    {
        if (additionalVolume <= 0)
            return Error.New("Additional volume cannot be zero.");
        var newVolume = currentVolume + additionalVolume;
        return FinSucc(newVolume);
    }
}
