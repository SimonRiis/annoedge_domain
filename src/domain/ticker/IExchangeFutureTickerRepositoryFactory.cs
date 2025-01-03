using LanguageExt;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace domain.ticker
{
    public interface IExchangeFutureTickerRepositoryFactory
    {
        Fin<IFutureTickerRepository> Get(Guid exchangeId);
    }
}
