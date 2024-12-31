using domain.exchange;
using LanguageExt;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace domain.product
{
    public interface IExchangeSymbolMapRepository
    {
        Task<Fin<string>> GetExchangeSymbolForProductAsync(Product product, Exchange exchange);
    }
}
