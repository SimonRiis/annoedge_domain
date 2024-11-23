using LanguageExt;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace domain.services.position
{
    public interface IPositionManagementService
    {
        Task<Fin<Unit>> UpdatePositions();
    }
}
