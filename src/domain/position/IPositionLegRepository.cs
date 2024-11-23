using domain.shared.repositories;
using LanguageExt;

namespace domain.position
{
    public interface IPositionLegRepository : IRepository<PositionLeg>
    {
        Task<Fin<IEnumerable<PositionLeg>>> GetAllForPosition(Guid positionId);
    }
}
