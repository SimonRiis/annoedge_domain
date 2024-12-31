using domain.product;
using LanguageExt;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace domain.ticker
{
    public interface IFutureTickerRepository
    {
        Task<Option<FutureTicker>> GetLatestTickerAsync(Product product, DateTimeOffset dateTimeOffset);
    }
}
