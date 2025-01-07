using domain.shared.repositories;
using LanguageExt;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace domain.position
{
    public interface IPositionLegIntentRepository : IRepository<PositionLegIntent>
    {
        Task<Fin<IEnumerable<PositionLegIntent>>> GetAllForPositionLeg(Guid legId);
    }
}
