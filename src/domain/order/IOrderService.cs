using domain.position;
using LanguageExt;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace domain.order
{
    public interface IOrderService
    {
        Task<Fin<IEnumerable<Fin<Order>>>> GetOrdersWithUnhandledFills();

        Task<Fin<Unit>> UpdateAsync(Order order);
    }
}
