using domain.shared;
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
        Task<IEnumerable<Quote>> GetQuotesAsync(Symbol symbol, DateTime startDate, DateTime endDate);
    }
}
