using System;
using domain.shared.repositories;
using LanguageExt;

namespace domain.order;

public interface IOrderRepository : IRepository<Order>
{
    Task<Fin<IEnumerable<Order>>> GetOrdersWithUnhandledFills();
}
