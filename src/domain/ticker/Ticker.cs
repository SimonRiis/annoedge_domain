using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using domain.shared;

namespace domain.ticker
{
    public record Ticker
    {
        public DateTimeOffset Timestamp { get; init; }
        public required Symbol Symbol { get; init; }
        public required string Exchange { get; init; }
        public required decimal BestAsk { get; init; }
        public required decimal BestBid { get; init; }
        public required decimal LastPrice { get; init; }
        public required decimal Volume { get; init; }

        public required decimal AskSize { get; init; }
        public required decimal BidSize { get; init; }

        public required bool Suspended { get; init; }
        public required bool PostOnly { get; init; }

    }

    public record FutureTicker : Ticker
    {
        public required decimal IndexPrice { get; init; }
        public required decimal MarkPrice { get; init; }
        public required decimal OpenInterest { get; init; }
    }
}
