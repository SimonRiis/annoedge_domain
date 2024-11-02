using System;
using LanguageExt;

namespace domain.position;

public interface IPositionLegService
{
    Fin<PositionLeg> IncreaseVolume(PositionLeg leg, decimal additionalVolume, decimal newEntryPrice);
    Fin<PositionLeg> DecreaseVolume(PositionLeg leg, decimal volumeReduction);  
}
