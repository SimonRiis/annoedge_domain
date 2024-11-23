using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
