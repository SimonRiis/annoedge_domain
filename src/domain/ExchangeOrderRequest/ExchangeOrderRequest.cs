using domain.shared.enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace domain.ExchangeOrderRequest
{
    //public enum OrderDirection { SELL, BUY }

    public record ExchangeOrderRequest
    {
        public required string OwnOrderId { get; init; }
        public required Guid ExchangeId { get; init; }
        public required Guid PositionId { get; init; }
        public required decimal Volume { get; init; }
        public required string Asset { get; init; }
     
        public required Direction OrderDirection { get; init; }
        public override string ToString()
        {
            return $"volume {Volume} orderdirection {OrderDirection.ToString()}";
        }
    }
}
