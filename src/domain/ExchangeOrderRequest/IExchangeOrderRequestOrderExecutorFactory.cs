using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace domain.ExchangeOrderRequest
{
    public interface IExchangeOrderRequestOrderExecutorFactory
    {
        IExchangeOrderRequestOrderExecutor Get(string exchange);
    }
}
