using LanguageExt;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace domain.position
{
    public interface IPositionEventService
    {
        Task<Fin<Unit>> SaveEvent(PositionEvent positionEvent);
        Task<Fin<PositionEvent>> GetLatestEvent(Guid positionId);
    }
}
