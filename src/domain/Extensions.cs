using LanguageExt;
using LanguageExt.Common;
using static LanguageExt.Prelude;

namespace domain
{
    public static class Extensions
    {
        // BindAsync: Monadic bind for asynchronous operations returning Task<Fin<U>>
        public static async Task<Fin<U>> BindAsync<T, U>(this Fin<T> fin, Func<T, Task<Fin<U>>> f) =>
            await fin.Match(
                Succ: t => f(t),
                Fail: ex => Task.FromResult(FinFail<U>(ex))
            );

        // MapAsync: Monadic map for asynchronous operations returning Task<U>
        public static async Task<Fin<U>> MapAsync<T, U>(this Fin<T> fin, Func<T, Task<U>> f) =>
            await fin.Match(
                Succ: async t => FinSucc(await f(t)),
                Fail: ex => Task.FromResult(FinFail<U>(ex))
            );

        // Convert Option<T> to Fin<T> with a failure message
        public static Fin<T> ToFin<T>(this Option<T> opt, string errorMsg) =>
            opt.Match(
                Some: FinSucc,
                None: () => FinFail<T>(new Exception(errorMsg))
            );
        public static Fin<B> BindOption<A, B>(this Fin<A> fin, Func<A, Option<B>> func, string errorMessage) =>
        fin.Bind(a => func(a).Match(FinSucc, () => FinFail<B>(errorMessage)));

        public static Fin<Unit> FlattenResult(this Fin<Unit>[] func) =>
            func.All(result => result.IsSucc)
                ? FinSucc(Unit.Default)
                : FinFail<Unit>(string.Join(", ", func.Fails().Select(r => r.Message)));

    }
}
