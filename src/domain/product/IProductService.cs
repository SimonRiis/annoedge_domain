using domain.exchange;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace domain.product
{
    public interface IProductService
    {
        Task<string> GetExchangeSymbolForProduct(Product product, Exchange exchange);
    }
}
