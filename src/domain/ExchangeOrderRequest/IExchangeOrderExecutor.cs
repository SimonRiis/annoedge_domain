using LanguageExt;

namespace domain.ExchangeOrderRequest
{
    public interface IExchangeOrderExecutor<T>
    {
        Task<Fin<ExchangeOrderRequestResponse>> ExecuteOrderRequest(T orderRequest);
    }
}
