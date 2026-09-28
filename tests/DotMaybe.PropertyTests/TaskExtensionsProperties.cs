using FsCheck.Xunit;

namespace DotMaybe.PropertyTests;

/// <summary>
/// The extensions on <c>Task&lt;Maybe&lt;T&gt;&gt;</c> and <c>ValueTask&lt;Maybe&lt;T&gt;&gt;</c> are "await the source,
/// then do what <see cref="Maybe{T}"/> does", for completed and pending sources of both kinds, with synchronous and
/// asynchronous delegates.
/// </summary>
public sealed class TaskExtensionsProperties
{
    [Property]
    public async Task MapAsync_agrees_with_Map(Maybe<int> maybe, SourceKind kind, Func<int, string> map, bool yields)
    {
        var expected = maybe.Map(map);
        Func<int, Task<string>> asyncMap = value => Deferred.Lift(map(value), yields);

        Assert.Equal(expected, await Sources.Apply(maybe, kind, s => s.MapAsync(map), s => s.MapAsync(map)));
        Assert.Equal(expected, await Sources.Apply(maybe, kind, s => s.MapAsync(asyncMap), s => s.MapAsync(asyncMap)));
    }

    [Property]
    public async Task BindAsync_agrees_with_Bind(
        Maybe<int> maybe,
        SourceKind kind,
        Func<int, Maybe<string>> bind,
        bool yields)
    {
        var expected = maybe.Bind(bind);
        Func<int, Task<Maybe<string>>> asyncBind = value => Deferred.Lift(bind(value), yields);

        Assert.Equal(expected, await Sources.Apply(maybe, kind, s => s.BindAsync(bind), s => s.BindAsync(bind)));
        Assert.Equal(
            expected,
            await Sources.Apply(maybe, kind, s => s.BindAsync(asyncBind), s => s.BindAsync(asyncBind)));
    }

    [Property]
    public async Task FilterAsync_agrees_with_Filter(
        Maybe<int> maybe,
        SourceKind kind,
        Func<int, bool> predicate,
        bool yields)
    {
        var expected = maybe.Filter(predicate);
        Func<int, Task<bool>> asyncPredicate = value => Deferred.Lift(predicate(value), yields);

        Assert.Equal(
            expected,
            await Sources.Apply(maybe, kind, s => s.FilterAsync(predicate), s => s.FilterAsync(predicate)));
        Assert.Equal(
            expected,
            await Sources.Apply(maybe, kind, s => s.FilterAsync(asyncPredicate), s => s.FilterAsync(asyncPredicate)));
    }

    [Property]
    public async Task MatchAsync_agrees_with_Match(
        Maybe<int> maybe,
        SourceKind kind,
        Func<int, string> some,
        string none,
        bool yields)
    {
        var expected = maybe.Match(some, () => none);
        Func<string> syncNone = () => none;
        Func<int, Task<string>> asyncSome = value => Deferred.Lift(some(value), yields);
        Func<Task<string>> asyncNone = () => Deferred.Lift(none, yields);

        Assert.Equal(
            expected,
            await Sources.Apply(maybe, kind, s => s.MatchAsync(some, syncNone), s => s.MatchAsync(some, syncNone)));
        Assert.Equal(
            expected,
            await Sources.Apply(
                maybe,
                kind,
                s => s.MatchAsync(asyncSome, asyncNone),
                s => s.MatchAsync(asyncSome, asyncNone)));
        Assert.Equal(
            expected,
            await Sources.Apply(
                maybe,
                kind,
                s => s.MatchAsync(asyncSome, syncNone),
                s => s.MatchAsync(asyncSome, syncNone)));
    }

    [Property]
    public async Task OrDefaultAsync_agrees_with_OrDefault(Maybe<int> maybe, SourceKind kind, int fallback, bool yields)
    {
        var expected = maybe.OrDefault(fallback);
        Func<int> syncFallback = () => fallback;
        Func<Task<int>> asyncFallback = () => Deferred.Lift(fallback, yields);

        Assert.Equal(
            expected,
            await Sources.Apply(maybe, kind, s => s.OrDefaultAsync(fallback), s => s.OrDefaultAsync(fallback)));
        Assert.Equal(
            expected,
            await Sources.Apply(maybe, kind, s => s.OrDefaultAsync(syncFallback), s => s.OrDefaultAsync(syncFallback)));
        Assert.Equal(
            expected,
            await Sources.Apply(maybe, kind, s => s.OrDefaultAsync(asyncFallback), s => s.OrDefaultAsync(asyncFallback)));
    }

    [Property]
    public async Task A_chain_of_steps_agrees_with_the_synchronous_chain(
        Maybe<int> maybe,
        SourceKind kind,
        Func<int, string> first,
        Func<string, Maybe<bool>> second,
        Func<bool, bool> third,
        bool yields)
    {
        var expected = maybe.Map(first).Bind(second).Filter(third).OrDefault(false);
        Func<string, Task<Maybe<bool>>> asyncSecond = value => Deferred.Lift(second(value), yields);

        var actual = await Sources.Apply(
            maybe,
            kind,
            s => s.MapAsync(first).BindAsync(asyncSecond).FilterAsync(third).OrDefaultAsync(false),
            s => s.MapAsync(first).BindAsync(asyncSecond).FilterAsync(third).OrDefaultAsync(false));

        Assert.Equal(expected, actual);
    }

    [Property]
    public async Task A_chain_can_start_from_a_plain_maybe(
        Maybe<int> maybe,
        Func<int, string> first,
        Func<string, Maybe<bool>> second,
        bool yields)
    {
        var expected = maybe.Map(first).Bind(second).OrDefault(false);

        var actual = await maybe
            .MapAsync(value => Deferred.Lift(first(value), yields))
            .BindAsync(second)
            .OrDefaultAsync(false);

        Assert.Equal(expected, actual);
    }

    [Property]
    public void A_completed_source_with_synchronous_steps_is_already_complete(Maybe<int> maybe, Func<int, string> map)
    {
        Assert.True(Task.FromResult(maybe).MapAsync(map).IsCompletedSuccessfully);
        Assert.True(ValueTask.FromResult(maybe).MapAsync(map).IsCompletedSuccessfully);
        Assert.True(Task.FromResult(maybe).MatchAsync(map, () => "none").IsCompletedSuccessfully);
        Assert.True(ValueTask.FromResult(maybe).OrDefaultAsync(0).IsCompletedSuccessfully);
    }

    [Fact]
    public async Task A_failed_source_fails_the_result_and_runs_nothing()
    {
        var calls = 0;
        Func<int, int> map = value => ++calls + value;
        var failure = new InvalidOperationException("source");

        var fromTask = Task.FromException<Maybe<int>>(failure).MapAsync(map);
        var fromValueTask = ValueTask.FromException<Maybe<int>>(failure).MapAsync(map);

        Assert.Same(failure, await Assert.ThrowsAsync<InvalidOperationException>(async () => await fromTask));
        Assert.Same(failure, await Assert.ThrowsAsync<InvalidOperationException>(async () => await fromValueTask));
        Assert.Equal(0, calls);
    }

    [Fact]
    public async Task A_canceled_source_cancels_the_result_and_runs_nothing()
    {
        using var source = new CancellationTokenSource();
        await source.CancelAsync();
        var calls = 0;
        Func<int, int> map = value => ++calls + value;

        var fromTask = Task.FromCanceled<Maybe<int>>(source.Token).MapAsync(map);
        var fromValueTask = ValueTask.FromCanceled<Maybe<int>>(source.Token).MapAsync(map);

        Assert.True(fromTask.IsCanceled);
        Assert.True(fromValueTask.IsCanceled);
        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () => await fromTask);
        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () => await fromValueTask);
        Assert.Equal(0, calls);
    }

    [Fact]
    public async Task A_throwing_step_fails_only_when_awaited()
    {
        Func<int, int> map = _ => throw new InvalidOperationException("step");

        ValueTask<Maybe<int>> pending = default;
        var thrownByTheCall = Record.Exception(() => { pending = Task.FromResult(Maybes.Some(1)).MapAsync(map); });

        Assert.Null(thrownByTheCall);
        var error = await Assert.ThrowsAsync<InvalidOperationException>(async () => await pending);
        Assert.Equal("step", error.Message);
    }

    [Fact]
    public async Task A_step_canceled_by_its_token_cancels_the_result()
    {
        using var source = new CancellationTokenSource();
        await source.CancelAsync();
        Func<int, int> map = value =>
        {
            source.Token.ThrowIfCancellationRequested();
            return value;
        };

        var pending = Task.FromResult(Maybes.Some(1)).MapAsync(map);

        Assert.True(pending.IsCanceled);
        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () => await pending);
    }

    [Fact]
    public void A_missing_task_source_is_rejected_immediately()
    {
        Task<Maybe<int>> source = null!;

        AssertRejects("source", () => source.MapAsync(value => value));
        AssertRejects("source", () => source.BindAsync(value => Maybes.Some(value)));
        AssertRejects("source", () => source.FilterAsync(_ => true));
        AssertRejects("source", () => source.MatchAsync(value => value, () => 0));
        AssertRejects("source", () => source.OrDefaultAsync(0));
    }

    [Fact]
    public void Missing_delegates_are_rejected_immediately_on_task_sources()
    {
        var numbers = Task.FromResult(Maybes.Some(1));
        var texts = Task.FromResult(Maybes.Some("text"));

        AssertRejects("map", () => numbers.MapAsync((Func<int, int>)null!));
        AssertRejects("map", () => numbers.MapAsync((Func<int, Task<int>>)null!));
        AssertRejects("bind", () => numbers.BindAsync((Func<int, Maybe<int>>)null!));
        AssertRejects("bind", () => numbers.BindAsync((Func<int, Task<Maybe<int>>>)null!));
        AssertRejects("predicate", () => numbers.FilterAsync((Func<int, bool>)null!));
        AssertRejects("predicate", () => numbers.FilterAsync((Func<int, Task<bool>>)null!));
        AssertRejects("some", () => numbers.MatchAsync((Func<int, int>)null!, () => 0));
        AssertRejects("none", () => numbers.MatchAsync(value => value, (Func<int>)null!));
        AssertRejects("some", () => numbers.MatchAsync((Func<int, Task<int>>)null!, () => Task.FromResult(0)));
        AssertRejects("none", () => numbers.MatchAsync(value => Task.FromResult(value), (Func<Task<int>>)null!));
        AssertRejects("some", () => numbers.MatchAsync((Func<int, Task<int>>)null!, () => 0));
        AssertRejects("none", () => numbers.MatchAsync(value => Task.FromResult(value), (Func<int>)null!));
        AssertRejects("fallback", () => texts.OrDefaultAsync((string)null!));
        AssertRejects("fallback", () => numbers.OrDefaultAsync((Func<int>)null!));
        AssertRejects("fallback", () => numbers.OrDefaultAsync((Func<Task<int>>)null!));
    }

    [Fact]
    public void Missing_delegates_are_rejected_immediately_on_value_task_sources()
    {
        var numbers = ValueTask.FromResult(Maybes.Some(1));
        var texts = ValueTask.FromResult(Maybes.Some("text"));

        AssertRejects("map", () => numbers.MapAsync((Func<int, int>)null!));
        AssertRejects("map", () => numbers.MapAsync((Func<int, Task<int>>)null!));
        AssertRejects("bind", () => numbers.BindAsync((Func<int, Maybe<int>>)null!));
        AssertRejects("bind", () => numbers.BindAsync((Func<int, Task<Maybe<int>>>)null!));
        AssertRejects("predicate", () => numbers.FilterAsync((Func<int, bool>)null!));
        AssertRejects("predicate", () => numbers.FilterAsync((Func<int, Task<bool>>)null!));
        AssertRejects("some", () => numbers.MatchAsync((Func<int, int>)null!, () => 0));
        AssertRejects("none", () => numbers.MatchAsync(value => value, (Func<int>)null!));
        AssertRejects("some", () => numbers.MatchAsync((Func<int, Task<int>>)null!, () => Task.FromResult(0)));
        AssertRejects("none", () => numbers.MatchAsync(value => Task.FromResult(value), (Func<Task<int>>)null!));
        AssertRejects("some", () => numbers.MatchAsync((Func<int, Task<int>>)null!, () => 0));
        AssertRejects("none", () => numbers.MatchAsync(value => Task.FromResult(value), (Func<int>)null!));
        AssertRejects("fallback", () => texts.OrDefaultAsync((string)null!));
        AssertRejects("fallback", () => numbers.OrDefaultAsync((Func<int>)null!));
        AssertRejects("fallback", () => numbers.OrDefaultAsync((Func<Task<int>>)null!));
    }

    private static void AssertRejects(string parameter, Func<object> call) =>
        Assert.Equal(parameter, Assert.Throws<ArgumentNullException>(call).ParamName);
}
