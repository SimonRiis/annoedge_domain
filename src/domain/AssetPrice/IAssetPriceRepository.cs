using domain.shared.repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace domain.AssetPrice
{
    public interface IAssetPriceRepository<T> : IRepository<T> where T : AssetPrice
    {
    }
}
