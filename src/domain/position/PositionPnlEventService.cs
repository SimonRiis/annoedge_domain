using LanguageExt;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace domain.position
{
    public class PositionPnlEventService(IPositionPnlEventRepository positionPnlEventRepository) : IPositionPnlEventService
    {
        public Task<Fin<Unit>> SavePnlEvent(PositionPnLEvent pnlEvent)
        {
            return positionPnlEventRepository.AddAsync(pnlEvent);
        }
        public Task<Fin<PositionPnLEvent>> GetLatestPnlEvent(Guid positionId)
        {
            return positionPnlEventRepository.GetLatestPnlEvent(positionId);
        }
    }
}
