using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace domain.AssetPrice
{
    public interface IAssetPriceService<T> where T : AssetPrice
    {
        Task<T> GetPriceAsync(string symbol, DateTimeOffset date);
    }
}
