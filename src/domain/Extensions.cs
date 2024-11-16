using LanguageExt;
using static LanguageExt.Prelude;

namespace domain
{
    internal static class Extensions
    {
        public static Fin<B> BindOption<A, B>(this Fin<A> fin, Func<A, Option<B>> func, string errorMessage) =>
        fin.Bind(a => func(a).Match(FinSucc, () => FinFail<B>(errorMessage)));

    }
}
