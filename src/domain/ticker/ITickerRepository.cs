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
        Task<Option<T>> GetLatestTickerAsync<T>(Product product, DateTimeOffset dateTimeOffset) where T : Ticker;
    }
}
