using domain.shared.enums;
using domain.shared.repositories;
using LanguageExt;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace domain.order
{
    public interface IFillStatusRepository : IRepository<FillStatus>
    {
        Task<Fin<FillStatus>> GetForFillId(string fillId);
        Task<Fin<Unit>> UpdateForFillId(string fillId, FillStatus status);

    }
}
