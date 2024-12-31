using domain.exchange;
using domain.position;
using domain.product;
using domain.shared.enums;
using LanguageExt;
using LanguageExt.Common;
using static LanguageExt.Prelude;
namespace domain.order;



public class Order(Product product, Exchange exchange, string id, string externalId, Status status, decimal quantity, decimal price, Direction direction, DateTimeOffset exchangeTimestamp, DateTimeOffset lastUpdated )
{
    public Direction Direction { get; init; } = direction;
    public string Id { get; init; } = id;
    public string ExternalId  { get; init; } = externalId;
    public Product Product { get; init; } = product;
    public decimal Quantity { get; init; } = quantity;
    public decimal Price { get; init; } = price;
    public Exchange Exchange { get; init; } = exchange;
    public Status Status { get; private set; } = status;
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

    public Option<Fill> GetFillById(string fillId) => _fills.Find(f => f.Id == fillId);

    private Fin<Unit> UpdateFill(Fill oldFill, Fill updatedFill)
    {
        if (oldFill == null)
            return Error.New("Fill not found");

        _fills[_fills.IndexOf(oldFill)] = updatedFill;
        return FinSucc(unit);
    }

    public Fin<Unit> UpdateFillStatus(string fillId, FillStatus status)
    {
        var fill = GetFillById(fillId);
        return fill.Match(
            Some: f => {

                var updatedFill = f with { Status = status };
                return UpdateFill(f, updatedFill);

            },
            None: () => Error.New($"Fill {fillId} not found.")
        );
    }

    private void UpdateOrderStatus()
    {
        decimal filledQuantity = 0;
        foreach (var fill in _fills)
            filledQuantity += fill.Quantity;

        if (filledQuantity == 0)
        {
            Status = Status.Pending;
        }
        else if (filledQuantity < Quantity)
        {
            Status = Status.PartiallyFilled;
        }
        else if (filledQuantity >= Quantity)
        {
            Status = Status.Filled;
        }
    }
}
