using domain.shared.repositories;
using LanguageExt;

namespace domain.algobook;

public interface IAlgoBookRepository : IRepository<AlgoBook>
{
    Task<Fin<IEnumerable<AlgoBook>>> GetAll();
}
