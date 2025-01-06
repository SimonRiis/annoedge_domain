using domain.position;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace domain.position
{
    public enum PositionEventType
    {
        SCALE_IN,
        OPEN,
        SCALE_OUT,
        CLOSE
    }

    public record PositionEvent
    {
        public required Guid Id { get; init; }
        public required Guid PositionId { get; init; }
        public required PositionEventType EventType { get; init; }
        public required PositionStatus OldStatus { get; init; }
        public required PositionStatus NewStatus { get; init; }
        public required string EventData { get; init; }
        public required DateTimeOffset CreatedAt { get; init; }
    }
}