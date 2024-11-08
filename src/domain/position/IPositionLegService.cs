using System;
using LanguageExt;

namespace domain.position;

public interface IPositionLegService
{
    Fin<PositionLeg> IncreaseQuantity(PositionLeg leg, decimal additionalQuantity, decimal newEntryPrice);
    Fin<PositionLeg> DecreaseQuantity(PositionLeg leg, decimal QuantityReduction);  
}
