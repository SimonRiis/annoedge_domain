using LanguageExt;
using Skender.Stock.Indicators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace domain.ExchangeOrderRequest
{
    public interface IExchangeOrderRequestValidator
    {
        Task<Fin<Unit>> ValidateAsync<T>(T orderRequest, IEnumerable<Quote> quotes) where T : ExchangeOrderRequest;
    }
}
