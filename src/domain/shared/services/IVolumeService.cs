using LanguageExt;

namespace domain.shared.services;

public interface IVolumeService
{
    public Fin<decimal> IncreaseVolume(decimal currentVolume, decimal additionalVolume);
    public Fin<decimal> DecreaseVolume(decimal currentVolume, decimal volumeReduction);
}
