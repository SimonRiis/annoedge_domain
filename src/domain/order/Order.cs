using domain.shared;
using LanguageExt;
using LanguageExt.Common;
using static LanguageExt.Prelude;
namespace domain.order;

public enum OrderStatus
{
    Pending,
    PartiallyFilled,
    Filled,
    Canceled
}

public class Order(Product product, Exchange exchange, Guid id, decimal quantity, decimal price, DateTimeOffset exchangeTimestamp, DateTimeOffset lastUpdated )
{
    public Guid Id { get; init; } = id;
    public Product Product { get; init; } = product;
    public decimal Quantity { get; init; } = quantity;
    public decimal Price { get; init; } = price;
    public Exchange Exchange { get; init; } = exchange;
    public OrderStatus Status { get; private set; } = OrderStatus.Pending;
    public DateTimeOffset LastUpdated { get; private set; } = lastUpdated;
    public DateTimeOffset ExchangeTimestamp { get; init; } = exchangeTimestamp;

    private readonly List<Trade> _trades = new List<Trade>();
    private readonly List<Fill> _fills = new List<Fill>();

    public IReadOnlyCollection<Trade> Trades => _trades.AsReadOnly();
    public IReadOnlyCollection<Fill> Fills => _fills.AsReadOnly();

    public Fin<Unit> AddTrade(Trade trade)
    {
        if (trade.OrderId != Id)
            return Error.New($"Trade with tradeId: {trade.Id} does not belong to this order.");

        _trades.Add(trade);
        UpdateOrderStatus();
        return FinSucc(unit);
    }

    public Fin<Unit> AddFill(Fill fill)
    {
        if (fill.OrderId != Id)
            return Error.New($"Fill with fillId: {fill.Id} does not belong to this order.");

        _fills.Add(fill);
        UpdateOrderStatus();
        return FinSucc(unit);
    }

    private void UpdateOrderStatus()
    {
        decimal filledQuantity = 0;
        foreach (var fill in _fills)
            filledQuantity += fill.Quantity;

        if (filledQuantity == 0)
        {
            Status = OrderStatus.Pending;
        }
        else if (filledQuantity < Quantity)
        {
            Status = OrderStatus.PartiallyFilled;
        }
        else if (filledQuantity >= Quantity)
        {
            Status = OrderStatus.Filled;
        }
    }
}
