using domain.shared;
using LanguageExt;
using Skender.Stock.Indicators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace domain.quote
{
    public interface IQuoteRepository
    {
        Task<Fin<IEnumerable<Quote>>> GetQuotesAsync(Symbol symbol, DateTimeOffset startDate, DateTimeOffset endDate);
    }
}
