namespace domain.shared.repositories
{
    public interface IProductRepository : IRepository<Product>
    {
        Task<Product> GetByAssetAndExchange(string asset, string exchange);
    }
}
