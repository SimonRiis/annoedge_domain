using LanguageExt;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace domain.position
{
    public interface IPositionPnlEventService
    {
        Task<Fin<Unit>> SavePnlEvent(PositionPnLEvent pnlEvent);
        Task<Fin<PositionPnLEvent>> GetLatestPnlEvent(Guid positionId);
    }
}
