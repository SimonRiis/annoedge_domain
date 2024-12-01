using System;
using domain.shared.repositories;
using LanguageExt;

namespace domain.order;

public interface IOrderRepository : IExternalDataRepository<Order>
{
    Task<Fin<IEnumerable<Order>>> GetOrdersWithUnhandledFills();
}
