using LanguageExt;

namespace domain.shared.repositories
{
    public interface IProductRepository : IRepository<Product>
    {
        Task<Option<Product>> GetByAssetAndExchange(string asset, Guid exchangeId);
    }
}
