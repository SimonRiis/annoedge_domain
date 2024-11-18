using domain.position;
using domain.shared;
using domain.shared.enums;
using LanguageExt;
using LanguageExt.Common;
using static LanguageExt.Prelude;
namespace domain.order;



public class Order(Product product, Exchange exchange, ExchangeMapId id, decimal quantity, decimal price, Direction direction, DateTimeOffset exchangeTimestamp, DateTimeOffset lastUpdated )
{
    public Direction Direction { get; init; } = direction;
    public ExchangeMapId Id { get; init; } = id;
    public Product Product { get; init; } = product;
    public decimal Quantity { get; init; } = quantity;
    public decimal Price { get; init; } = price;
    public Exchange Exchange { get; init; } = exchange;
    public Status Status { get; private set; } = Status.Pending;
    public DateTimeOffset LastUpdated { get; private set; } = lastUpdated;
    public DateTimeOffset ExchangeTimestamp { get; init; } = exchangeTimestamp;

    private readonly List<Trade> _trades = new List<Trade>();
    private readonly List<Fill> _fills = new List<Fill>();

    public IReadOnlyCollection<Trade> Trades => _trades.AsReadOnly();
    public IReadOnlyCollection<Fill> Fills => _fills.AsReadOnly();

    public Fin<Unit> AddTrade(Trade trade)
    {
        if (trade.OrderId.InternalId != Id.InternalId)
            return Error.New($"Trade with tradeId: {trade.Id} does not belong to this order.");

        _trades.Add(trade);
        UpdateOrderStatus();
        return FinSucc(unit);
    }

    public Fin<Unit> AddFill(Fill fill)
    {
        if (fill.OrderId.InternalId != Id.InternalId)
            return Error.New($"Fill with fillId: {fill.Id} does not belong to this order.");

        _fills.Add(fill);
        UpdateOrderStatus();
        return FinSucc(unit);
    }

    public Option<Fill> GetFillById(Guid fillId) => _fills.Find(f => f.Id.InternalId == fillId);

    private Fin<Unit> UpdateFill(Fill oldFill, Fill updatedFill)
    {
        if (oldFill == null)
            return Error.New("Fill not found");

        _fills[_fills.IndexOf(oldFill)] = updatedFill;
        return FinSucc(unit);
    }

    public Fin<Unit> UpdateFillStatus(Guid fillId, FillStatus status)
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
