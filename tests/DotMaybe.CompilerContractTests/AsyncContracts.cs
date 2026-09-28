namespace DotMaybe.CompilerContractTests;

/// <summary>
/// How the async API reads at the call site: async lambdas and Task-returning methods fit without type
/// arguments, both MatchAsync overloads are chosen without ambiguity, and ValueTask-returning delegates are out of
/// scope (D31: delegates return Task, methods return ValueTask).
/// </summary>
public sealed class AsyncContracts
{
    [Fact]
    public void An_async_pipeline_reads_without_type_arguments()
    {
        var result = Snippet.Compile(Consumer.File("""
                private static Task<Maybe<int>> FindAsync(string key) =>
                    Task.FromResult<Maybe<int>>(key.Length > 0 ? key.Length : none);

                private static async Task<string> LoadAsync(int id)
                {
                    await Task.Yield();
                    return $"item {id}";
                }

                public static async Task<string> Pipeline(Maybe<string> key)
                {
                    var found = await key.BindAsync(FindAsync);
                    var loaded = await found.MapAsync(LoadAsync);
                    var kept = await loaded.FilterAsync(async text =>
                    {
                        await Task.Yield();
                        return text.Length > 0;
                    });

                    return await kept.MatchAsync(
                        async text =>
                        {
                            await Task.Yield();
                            return text;
                        },
                        () => "nothing");
                }
            """));

        ContractAssert.CompilesCleanly(result);
    }

    [Fact]
    public void MatchAsync_takes_a_synchronous_or_an_asynchronous_none_branch()
    {
        var result = Snippet.Compile(Consumer.File("""
                public static ValueTask<string> Sync(Maybe<int> maybe) =>
                    maybe.MatchAsync(value => Task.FromResult($"{value}"), () => "none");

                public static ValueTask<string> Async(Maybe<int> maybe) =>
                    maybe.MatchAsync(value => Task.FromResult($"{value}"), () => Task.FromResult("none"));

                public static ValueTask<string> Named(Maybe<int> maybe) =>
                    maybe.MatchAsync(some: value => Task.FromResult($"{value}"), none: () => "none");

                public static ValueTask<int> Fallback(Maybe<int> maybe) =>
                    maybe.OrDefaultAsync(fallback: () => Task.FromResult(0));
            """));

        ContractAssert.CompilesCleanly(result);
    }

    [Fact]
    public void ValueTask_returning_delegates_are_not_accepted()
    {
        var result = Snippet.Compile(Consumer.File("""
                public static ValueTask<Maybe<int>> Broken(Maybe<int> maybe) =>
                    maybe.MapAsync(value => new ValueTask<int>(value));
            """));

        ContractAssert.DoesNotCompile(result);
    }
}
