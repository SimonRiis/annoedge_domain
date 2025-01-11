using domain.ExchangeOrderRequest;
using LanguageExt;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace domain.ExchangeOrderRequest
{
    public interface IExchangeOrderRequestValidatorFactory
    {
        Fin<IExchangeOrderRequestValidator> Get(Guid exchangeId, ExchangeOrderRequestType requestType);
    }
}
