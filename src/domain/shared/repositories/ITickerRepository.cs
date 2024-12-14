using LanguageExt;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace domain.shared.repositories
{
    public interface ITickerRepository

    {
        Task<Option<Ticker>> GetLatestTickerAsync(Symbol symbol, Exchange exchange, DateTimeOffset dateTimeOffset);
    }
}
