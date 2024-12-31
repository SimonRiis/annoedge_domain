using domain.exchange;
using LanguageExt;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace domain.product
{
    public class ProductService(IExchangeSymbolMapRepository exchangeSymbolMapRepository) : IProductService
    {
        public Task<Fin<string>> GetExchangeSymbolForProduct(Product product, Exchange exchange)
        {
            return exchangeSymbolMapRepository.GetExchangeSymbolForProductAsync(product, exchange);
        }
    }
}
