using Skender.Stock.Indicators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace domain.shared.repositories
{
    public interface IQuoteRepository
    {
        Task<IEnumerable<Quote>> GetQuotesAsync(string symbol, DateTime startDate, DateTime endDate);
    }
}
