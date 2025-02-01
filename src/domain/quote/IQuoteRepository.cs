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
        /// <summary>
        /// Returns a list of quotes for the given product and date range.
        /// </summary>
        /// <param name="product"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <returns></returns>
        Task<Fin<IEnumerable<Quote>>> GetQuotesAsync(Product product, DateTimeOffset startDate, DateTimeOffset endDate);
        /// <summary>
        /// Returns a list of quotes for the given exchange symbol and date range.
        /// It is the responsibility of the caller to map the quotes to a product.
        /// </summary>
        /// <param name="exchangesymbol"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <returns></returns>
        Task<Fin<IEnumerable<Quote>>> GetQuotesUnmappedAsync(string exchangesymbol, DateTimeOffset startDate, DateTimeOffset endDate);
        Task<Fin<IEnumerable<Quote>>> GetQuotesUnmappedAsync(DateTimeOffset startDate, DateTimeOffset endDate);
        Task<Fin<IEnumerable<string>>> GetDistinctSymbolsAsync();
    }
}
