using domain.order;
using domain.position;
using LanguageExt;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace domain.services.order
{
    public interface IOrderService
    {
        Fin<Order> CreateFrom(OrderDTO order);
    }
}
