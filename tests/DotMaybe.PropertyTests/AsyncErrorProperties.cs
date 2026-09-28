using FsCheck.Xunit;

namespace DotMaybe.PropertyTests;

/// <summary>
/// One rule for every async method (D32): only argument validation throws when the method is called. Whatever a
/// delegate does, including throwing before it returns a task, comes out when the result is awaited.
/// </summary>
public sealed class AsyncErrorProperties
{
    private const string Message = "thrown by the delegate";

    [Property]
    public async Task MatchAsync_reports_a_throwing_some_branch_when_awaited(int value)
    {
        Func<int, Task<int>> some = _ => throw Failure();
        Func<Task<int>> asyncNone = () => Task.FromResult(0);
        Func<int> syncNone = () => 0;

        await AssertFailsOnlyWhenAwaited(() => Maybes.Some(value).MatchAsync(some, asyncNone));
        await AssertFailsOnlyWhenAwaited(() => Maybes.Some(value).MatchAsync(some, syncNone));
    }

    [Fact]
    public async Task MatchAsync_reports_a_throwing_none_branch_when_awaited()
    {
        Func<int, Task<int>> some = value => Task.FromResult(value);
        Func<Task<int>> asyncNone = () => throw Failure();
        Func<int> syncNone = () => throw Failure();

        await AssertFailsOnlyWhenAwaited(() => Maybes.Empty<int>().MatchAsync(some, asyncNone));
        await AssertFailsOnlyWhenAwaited(() => Maybes.Empty<int>().MatchAsync(some, syncNone));
    }

    [Fact]
    public async Task OrDefaultAsync_reports_a_throwing_fallback_when_awaited()
    {
        Func<Task<int>> fallback = () => throw Failure();

        await AssertFailsOnlyWhenAwaited(() => Maybes.Empty<int>().OrDefaultAsync(fallback));
    }

    [Property]
    public async Task MapAsync_reports_a_throwing_step_when_awaited(int value)
    {
        Func<int, Task<string>> map = _ => throw Failure();

        await AssertFailsOnlyWhenAwaited(() => Maybes.Some(value).MapAsync(map));
    }

    [Property]
    public async Task BindAsync_reports_a_throwing_step_when_awaited(int value)
    {
        Func<int, Task<Maybe<string>>> bind = _ => throw Failure();

        await AssertFailsOnlyWhenAwaited(() => Maybes.Some(value).BindAsync(bind));
    }

    [Property]
    public async Task FilterAsync_reports_a_throwing_predicate_when_awaited(int value)
    {
        Func<int, Task<bool>> predicate = _ => throw Failure();

        await AssertFailsOnlyWhenAwaited(() => Maybes.Some(value).FilterAsync(predicate));
    }

    private static InvalidOperationException Failure() => new(Message);

    private static async Task AssertFailsOnlyWhenAwaited<TResult>(Func<ValueTask<TResult>> call)
    {
        ValueTask<TResult> pending = default;

        var thrownByTheCall = Record.Exception(() => { pending = call(); });

        Assert.Null(thrownByTheCall);
        var error = await Assert.ThrowsAsync<InvalidOperationException>(async () => await pending);
        Assert.Equal(Message, error.Message);
    }
}
