using domain.shared.repositories;
using LanguageExt;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace domain.position
{
    public interface IPositionEventRepository : IRepository<PositionEvent>
    {
        Task<Fin<IEnumerable<PositionEvent>>> GetAllForPosition(Guid positionId);

        Task<Fin<PositionEvent>> GetLatestEvent(Guid positionId);
    }
}
