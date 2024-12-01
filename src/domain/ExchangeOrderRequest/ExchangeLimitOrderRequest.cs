using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace domain.ExchangeOrderRequest
{
    public record ExchangeLimitOrderRequest : ExchangeOrderRequest
    {
        public decimal Price { get; set; }
    }
}
