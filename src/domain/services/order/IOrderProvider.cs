using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace domain.services.order
{
    public interface IOrderProvider
    {
        public Task<IEnumerable<OrderDTO>> GetOrders();
    }
}
