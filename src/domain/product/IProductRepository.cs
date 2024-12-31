using domain.shared.repositories;
using LanguageExt;

namespace domain.product
{
    public interface IProductRepository : IRepository<Product>
    {
        Task<Option<Product>> GetByAssetAndExchange(string asset, Guid exchangeId);
    }
}
