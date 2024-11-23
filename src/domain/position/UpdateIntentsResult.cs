namespace domain.position
{
    public class UpdateIntentsResult
    {
        public PositionLeg UpdatedLeg { get; }
        public decimal RemainingQuantity { get; }
        public UpdateIntentsResult(PositionLeg updatedLeg, decimal remainingQuantity)
        {
            UpdatedLeg = updatedLeg;
            RemainingQuantity = remainingQuantity;
        }
    }

}
