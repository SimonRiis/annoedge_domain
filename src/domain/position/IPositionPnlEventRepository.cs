using domain.shared.repositories;
using LanguageExt;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace domain.position
{
    public interface IPositionPnlEventRepository : IRepository<PositionPnLEvent>
    {
        Task<Fin<IEnumerable<PositionPnLEvent>>> GetAllForPosition(Guid positionId);

        Task<Fin<PositionPnLEvent>> GetLatestPnlEvent(Guid positionId);
    }
}
