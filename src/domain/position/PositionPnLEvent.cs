using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace domain.position
{
    public record PositionPnLEvent
    {
        public required Guid Id { get; init; }
        public required Guid PositionId { get; init; }
        public required decimal Pnl { get; init; }
        public required decimal PnlPct { get; init; }
        public required DateTimeOffset Timestamp { get; init; }
    }
}
