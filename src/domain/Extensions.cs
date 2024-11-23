using LanguageExt;
using LanguageExt.Common;
using static LanguageExt.Prelude;

namespace domain
{
    internal static class Extensions
    {
        public static Fin<B> BindOption<A, B>(this Fin<A> fin, Func<A, Option<B>> func, string errorMessage) =>
        fin.Bind(a => func(a).Match(FinSucc, () => FinFail<B>(errorMessage)));

        public static Fin<Unit> FlattenResult(this Fin<Unit>[] func) =>
            func.All(result => result.IsSucc)
                ? FinSucc(Unit.Default)
                : FinFail<Unit>(string.Join(", ", func.Fails().Select(r => r.Message)));

    }
}
