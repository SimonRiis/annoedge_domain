using LanguageExt;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace domain.ExchangeOrderRequest
{
    public interface IExchangeOrderRequestOrderExecutorFactory
    {
        Fin<IExchangeOrderRequestOrderExecutor> Get(Guid exchangeId);
    }
}
