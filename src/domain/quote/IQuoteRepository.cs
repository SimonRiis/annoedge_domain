using domain.product;
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
        Task<Fin<IEnumerable<Quote>>> GetQuotesAsync(Product product, DateTimeOffset startDate, DateTimeOffset endDate);
    }
}
