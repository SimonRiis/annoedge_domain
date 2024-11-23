using LanguageExt;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace domain.services.book
{
    public interface IBookVerifier
    {
        Task<Fin<Unit>> VerifyBooks();
    }
}
