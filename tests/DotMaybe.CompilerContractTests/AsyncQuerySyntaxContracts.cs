namespace DotMaybe.CompilerContractTests;

/// <summary>
/// Which query expressions work over awaitable maybes. The declared return types pin that the query is a
/// <c>ValueTask</c> of a maybe, so an asynchronous step never ends up as a value inside a synchronous maybe.
/// </summary>
public sealed class AsyncQuerySyntaxContracts
{
    [Fact]
    public void The_readme_async_query_compiles()
    {
        var result = Snippet.Compile(Consumer.File("""
                public sealed record User(string Name);

                public sealed record Order(decimal Total);

                private static Task<Maybe<User>> FindUserAsync(int id) =>
                    Task.FromResult<Maybe<User>>(id > 0 ? new User($"user {id}") : none);

                private static Task<Maybe<Order>> LastOrderAsync(User user) =>
                    Task.FromResult<Maybe<Order>>(new Order(user.Name.Length));

                public static async Task<string> Report(int id) =>
                    await (
                        from user in FindUserAsync(id)
                        from order in LastOrderAsync(user)
                        where order.Total > 0
                        select $"{user.Name}: {order.Total}")
                    .OrDefaultAsync("no orders");
            """));

        ContractAssert.CompilesCleanly(result);
    }

    [Fact]
    public void A_query_can_start_from_a_plain_maybe_and_mix_sync_and_async_steps()
    {
        var result = Snippet.Compile(Consumer.File("""
                private static Task<Maybe<string>> FindAsync(int id) =>
                    Task.FromResult<Maybe<string>>(id > 0 ? $"user {id}" : none);

                private static Maybe<int> Parse(string text) => int.TryParse(text, out var number) ? number : none;

                public static ValueTask<Maybe<string>> AsyncSecond(Maybe<int> id) =>
                    from i in id
                    from name in FindAsync(i)
                    let upper = name.ToUpperInvariant()
                    from length in Parse(upper)
                    where length > 0
                    select $"{upper}:{length}";

                public static ValueTask<Maybe<string>> AsyncThird(Maybe<string> text) =>
                    from t in text
                    from id in Parse(t)
                    from name in FindAsync(id)
                    select name;
            """));

        ContractAssert.CompilesCleanly(result);
    }

    [Fact]
    public void Value_task_sources_take_the_same_clauses()
    {
        var result = Snippet.Compile(Consumer.File("""
                private static Task<Maybe<int>> CountAsync(string text) =>
                    Task.FromResult<Maybe<int>>(text.Length);

                public static ValueTask<Maybe<int>> Selected(ValueTask<Maybe<int>> source) =>
                    from x in source
                    where x > 0
                    let doubled = x * 2
                    select doubled;

                public static ValueTask<Maybe<int>> Chained(ValueTask<Maybe<string>> source) =>
                    from text in source
                    from count in CountAsync(text)
                    select count;
            """));

        ContractAssert.CompilesCleanly(result);
    }

    [Fact]
    public void Value_task_returning_steps_are_not_supported()
    {
        // D31: steps are Task-returning, like every other async delegate in the library. A ValueTask source is fine.
        var result = Snippet.Compile(Consumer.File("""
                private static ValueTask<Maybe<int>> CountAsync(string text) =>
                    ValueTask.FromResult<Maybe<int>>(text.Length);

                public static object Query(Task<Maybe<string>> source) =>
                    from text in source
                    from count in CountAsync(text)
                    select count;
            """));

        ContractAssert.DoesNotCompile(result);
    }

    [Fact]
    public void Ordering_is_not_supported_in_async_queries()
    {
        var result = Snippet.Compile(Consumer.File("""
                public static object Ordered(Task<Maybe<int>> source) =>
                    from x in source
                    orderby x
                    select x;
            """));

        ContractAssert.FailsMentioning(result, "'OrderBy'");
    }
}
