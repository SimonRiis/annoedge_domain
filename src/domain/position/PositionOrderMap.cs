using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace domain.position
{
    public record PositionOrderMap
    {
        public Guid PositionId { get; init; }
        public Guid OrderId { get; init; }
    }
}
