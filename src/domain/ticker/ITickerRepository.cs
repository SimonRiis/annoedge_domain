using domain.shared;
using LanguageExt;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace domain.ticker
{
    public interface ITickerRepository

    {
        Task<Option<Ticker>> GetLatestTickerAsync(Product product, DateTimeOffset dateTimeOffset);
    }
}
