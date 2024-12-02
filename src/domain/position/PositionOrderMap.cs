using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace domain.position
{
    public record PositionOrderMap
    {
        public required Guid PositionId { get; init; }
        public required string OrderId { get; init; }
    }
}
