using domain.exchange;
using domain.product;
using domain.shared.enums;
using LanguageExt;
using LanguageExt.Common;
using static LanguageExt.Prelude;
namespace domain.position;

public record PositionLeg
{
    public required Guid Id { get; init; }
    public required Guid PositionId { get; init; }
    public required Guid BookId { get; init; }
    public required decimal Quantity { get; init; }
    public required decimal AverageEntryPrice { get; init; }
    public required DateTimeOffset LastUpdated { get; init; }
    public required Product Product { get; init; }
    public required Direction Direction { get; init; }
    public required Exchange Exchange { get; init; }
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

                var newStatus = newQuantity == 0 ? Status.Filled : Status.PartiallyFilled; // If the new quantity is 0, the status should be Filled

                var newIntent = i with { Quantity = newQuantity, Status = newStatus };

                _intents[_intents.IndexOf(i)] = newIntent;
                return FinSucc(this with { _intents = _intents }); // Return a new PositionLeg with the updated intents

            },
            None: () => FinFail<PositionLeg>("Intent not found")
        );
    }

    public Fin<decimal> GetOpenUnfilledQuantity()
    {
        var unfilledQuantity = _intents.Where(i => i.Status != Status.Filled && i.Status != Status.Canceled).Sum(i => i.Quantity);
        return FinSucc(unfilledQuantity);
    }
}

