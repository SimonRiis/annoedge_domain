namespace domain.shared;

public record ProductInfo
{
    public required string AssetType { get; init; }
    public decimal? TickSize { get; init; }
    public DateTimeOffset? ExpirationDate { get; init; }
    public decimal? MarginRequirement { get; init; }
    public required decimal MinOrderQtySize { get; init; }
    public required string BaseCurrency {get; init;}
    public required string QuoteCurrency {get; init;}
    public required string SettlementCurrency {get; init;}
    public required DateTimeOffset LastUpdated { get; init; }
}
