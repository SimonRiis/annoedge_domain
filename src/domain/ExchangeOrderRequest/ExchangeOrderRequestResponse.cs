using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace domain.ExchangeOrderRequest
{
    public class ExchangeOrderRequestResponse
    {
        public required string OrderId { get; set; }
        public required string ExchangeRawResponse { get; set; }
    }
}
