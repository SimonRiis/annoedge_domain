using System;
using domain.order;
using domain.shared.repositories;
using LanguageExt;

namespace domain.exchangebook;

public interface IExchangeBookRepository : IRepository<ExchangeBook>
{
    Task<Fin<IEnumerable<ExchangeBook>>> GetAll();
}
