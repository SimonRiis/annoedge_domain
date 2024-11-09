using domain.exchangebook;
using domain.globalbook;
using domain.order;
using LanguageExt;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace domain.services.book
{
    public interface IExchangeBooksService
    {
        Task<Fin<IEnumerable<ExchangeBook>>> CreateFrom(IEnumerable<Order> orders, CancellationToken cancellationToken);
    }
}
