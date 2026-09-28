namespace DotMaybe.CompilerContractTests;

/// <summary>
/// Async pipelines on awaitable maybes read without an await per step, and overload resolution picks the
/// asynchronous overload for Task-returning lambdas (otherwise the result type below would not match).
/// </summary>
public sealed class TaskExtensionsContracts
{
    [Fact]
    public void A_pipeline_from_a_task_returning_method_needs_one_await()
    {
        var result = Snippet.Compile(Consumer.File("""
                private static Task<Maybe<string>> FindAsync(int id) =>
                    Task.FromResult<Maybe<string>>(id > 0 ? $"user {id}" : none);

                private static Task<Maybe<int>> CountAsync(string name) =>
                    Task.FromResult<Maybe<int>>(name.Length);

                public static async Task<int> Pipeline(int id) =>
                    await FindAsync(id)
                        .MapAsync(name => name.ToUpperInvariant())
                        .BindAsync(CountAsync)
                        .FilterAsync(count => count > 3)
                        .OrDefaultAsync(0);
            """));

        ContractAssert.CompilesCleanly(result);
    }

    [Fact]
    public void Task_returning_lambdas_select_the_asynchronous_overloads()
    {
        var result = Snippet.Compile(Consumer.File("""
                public static ValueTask<Maybe<int>> Next(Task<Maybe<int>> source) =>
                    source.MapAsync(value => Task.FromResult(value + 1));

                public static ValueTask<int> Reduce(ValueTask<Maybe<int>> source) =>
                    source.MatchAsync(value => Task.FromResult(value), () => Task.FromResult(0));

                public static ValueTask<int> Fallback(ValueTask<Maybe<int>> source) =>
                    source.OrDefaultAsync(() => Task.FromResult(0));
            """));

        ContractAssert.CompilesCleanly(result);
    }

    [Fact]
    public void Async_lambdas_select_the_asynchronous_overloads()
    {
        var result = Snippet.Compile(Consumer.File("""
                public static ValueTask<Maybe<string>> Next(Task<Maybe<int>> source) =>
                    source.MapAsync(async value =>
                    {
                        await Task.Yield();
                        return $"{value}";
                    });
            """));

        ContractAssert.CompilesCleanly(result);
    }
}
