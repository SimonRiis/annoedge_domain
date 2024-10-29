using System;
using System.Xml.XPath;
using LanguageExt;
using LanguageExt.Common;
using static LanguageExt.Prelude;
namespace domain.position;

public class PositionLeg(Symbol symbol, PositionLegType legType, Guid legId, Guid positionId, decimal volume, decimal averageEntryPrice, Exchange exchange, DateTimeOffset lastUpdated) : IEquatable<PositionLeg>
{
    public Guid LegId { get; init; } = legId;
    public Guid PositionId { get; init; } = positionId;
    public decimal Volume { get; init; } = volume;
    public decimal AverageEntryPrice { get; init; } = averageEntryPrice;
    public DateTimeOffset LastUpdated { get; init; } = lastUpdated;
    public Symbol Symbol { get; init; } = symbol;
    public PositionLegType LegType { get; init; } = legType;
    public Exchange Exchange { get; init; } = exchange;

    public Fin<PositionLeg> IncreaseVolume(decimal additionalVolume, decimal newEntryPrice)
    {
        if (additionalVolume <= 0)
            return Error.New("Additional volume cannot be zero.");

        var newAverageEntryPrice = ((Volume * AverageEntryPrice) + (additionalVolume * newEntryPrice)) / (Volume + additionalVolume);

        return FinSucc(new PositionLeg(
            Symbol,
            LegType,
            LegId,
            PositionId,
            Volume + additionalVolume,
            newAverageEntryPrice,
            Exchange,
            DateTimeOffset.UtcNow
        ));
    }

    public bool Equals(PositionLeg? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;

        return LegId == other.LegId && Exchange == other.Exchange;
    }

    // Override Equals(object obj) for general equality comparison
    public override bool Equals(object? obj) =>
        obj is PositionLeg other && Equals(other);

    // Override GetHashCode to match the equality logic in Equals
    public override int GetHashCode()
    {
        return HashCode.Combine(LegId, Exchange);
    }

    public Fin<PositionLeg> DecreaseVolume(decimal decreaseVolume)
    {
        if (decreaseVolume <= 0)
            return Error.New("Decrease volume cannot be zero.");

        if (decreaseVolume > Volume)
            return Error.New("Decrease volume cannot exceed current position volume.");

        // Calculate the new volume after decreasing
        var newVolume = Volume - decreaseVolume;

        // Keep the same AverageEntryPrice (since exiting doesn’t affect the entry price of the remaining position)
        return FinSucc(new PositionLeg(
            Symbol,
            LegType,
            LegId,
            PositionId,
            newVolume,
            AverageEntryPrice,  // Retain the original average entry price
            Exchange,
            DateTimeOffset.UtcNow
        ));
    }
}

