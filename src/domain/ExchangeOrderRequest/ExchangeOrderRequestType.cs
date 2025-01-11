using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace domain.ExchangeOrderRequest
{
    public enum ExchangeOrderRequestType
    {
        LIMIT,
        MARKET,
        STOP_LIMIT,
        STOP_MARKET
    }
}
