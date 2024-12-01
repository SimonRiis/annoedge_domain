using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace domain.AssetPrice
{

    public record AssetPrice {

        public DateTime Timestamp { get; init; }
    }

    public record AssetFuturePrice : AssetPrice
    {
        public decimal BestAskPrice { get; init; }
        public decimal BestBidPrice { get; init; }
        public decimal LastPrice { get; init; }
        public decimal MarkPrice { get; init; }
        public decimal IndexPrice { get; init; }

    }
}
