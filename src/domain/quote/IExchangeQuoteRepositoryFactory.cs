using domain.shared.enums;
using LanguageExt;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace domain.quote
{
    public interface IExchangeQuoteRepositoryFactory
    {
        Fin<IQuoteRepository> Get(Guid exchangeId, SourceDataType sourceDataType);
    }
}
