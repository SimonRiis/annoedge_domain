using domain.shared;
using domain.shared.enums;
using LanguageExt;
using LanguageExt.Common;
using static LanguageExt.Prelude;
namespace domain.position;

public record PositionLeg(Guid id, Guid bookId, decimal quantity, decimal averageEntryPrice, DateTimeOffset lastUpdated, Product product, Direction direction, Exchange exchange)
{
    public required Guid Id { get; init; } = id;
    public required Guid BookId { get; init; } = bookId;
    public required decimal Quantity { get; init; } = quantity;
    public required decimal AverageEntryPrice { get; init; } = averageEntryPrice;
    public required DateTimeOffset LastUpdated { get; init; } = lastUpdated;
    public required Product Product { get; init; } = product;
    public required Direction Direction { get; init; } = direction;
    public required Exchange Exchange { get; init; } = exchange;
    private List<PositionLegIntent> _intents = [];
    public IReadOnlyCollection<PositionLegIntent> PositionLegIntents => _intents.AsReadOnly();

    public Fin<PositionLeg> AddIntent(PositionLegIntent intent)
    {
        _intents.Add(intent);
        return FinSucc(this with { _intents = _intents }); // Return a new PositionLeg with the updated intents
    }
    public Option<PositionLegIntent> GetLegIntentById(Guid intentId) => _intents.Find(i => i.Id == intentId);
    public Fin<PositionLeg> DecreaseIntentQuantity(PositionLegIntent intent, decimal decreaseQuantity)
    {
        var existingIntent = GetLegIntentById(intent.Id);

        return 
            existingIntent.Match(
            Some: i => {

                var newQuantity = i.Quantity - decreaseQuantity;

                newQuantity = newQuantity <= 0 ? 0 : newQuantity; // Ensure the new quantity is not negative

                var newStatus = newQuantity == 0 ? Status.Filled : i.Status; // If the new quantity is 0, the status should be Filled

                var newIntent = i with { Quantity = newQuantity, Status = newStatus };

                _intents[_intents.IndexOf(i)] = newIntent;
                return FinSucc(this with { _intents = _intents }); // Return a new PositionLeg with the updated intents

            },
            None: () => FinFail<PositionLeg>("Intent not found")
        );
    }

    public Fin<decimal> GetOpenUnfilledQuantity()
    {
        var unfilledQuantity = _intents.Where(i => i.Status != Status.Filled).Sum(i => i.Quantity);
        return FinSucc(unfilledQuantity);
    }
}

