using FsCheck.Xunit;
using static DotMaybe.Prelude;

namespace DotMaybe.PropertyTests;

/// <summary>
/// Query expressions over awaitable maybes mean the same as the synchronous query over the awaited maybes, for
/// completed and pending sources of both kinds, with synchronous and asynchronous steps. A query may also start
/// from a plain maybe and continue with an asynchronous step.
/// </summary>
public sealed class AsyncQuerySyntaxProperties
{
    [Property]
    public async Task Select_clause_agrees_with_Map(Maybe<int> maybe, SourceKind kind, Func<int, string> map)
    {
        var actual = await Sources.Apply(
            maybe,
            kind,
            s => from value in s select map(value),
            s => from value in s select map(value));

        Assert.Equal(maybe.Map(map), actual);
    }

    [Property]
    public async Task Where_clause_agrees_with_Filter(Maybe<int> maybe, SourceKind kind, Func<int, bool> predicate)
    {
        var actual = await Sources.Apply(
            maybe,
            kind,
            s => from value in s where predicate(value) select value,
            s => from value in s where predicate(value) select value);

        Assert.Equal(maybe.Filter(predicate), actual);
    }

    [Property]
    public async Task Two_from_clauses_agree_with_Bind_then_Map(
        Maybe<int> maybe,
        SourceKind kind,
        Func<int, Maybe<string>> next,
        Func<int, string, bool> combine,
        bool yields)
    {
        var expected = maybe.Bind(first => next(first).Map(second => combine(first, second)));
        Func<int, Task<Maybe<string>>> asyncNext = value => Deferred.Lift(next(value), yields);

        var withSyncStep = await Sources.Apply(
            maybe,
            kind,
            s => from first in s from second in next(first) select combine(first, second),
            s => from first in s from second in next(first) select combine(first, second));
        var withAsyncStep = await Sources.Apply(
            maybe,
            kind,
            s => from first in s from second in asyncNext(first) select combine(first, second),
            s => from first in s from second in asyncNext(first) select combine(first, second));

        Assert.Equal(expected, withSyncStep);
        Assert.Equal(expected, withAsyncStep);
    }

    [Property]
    public async Task Let_clause_binds_an_intermediate_value(Maybe<int> maybe, SourceKind kind, Func<int, string> map)
    {
        var actual = await Sources.Apply(
            maybe,
            kind,
            s => from value in s let text = map(value) select $"{value}:{text}",
            s => from value in s let text = map(value) select $"{value}:{text}");

        Assert.Equal(maybe.Map(value => $"{value}:{map(value)}"), actual);
    }

    [Property]
    public async Task A_query_mixing_sync_and_async_steps_agrees_with_the_synchronous_query(
        Maybe<int> maybe,
        SourceKind kind,
        Func<int, Maybe<string>> first,
        Func<string, Maybe<bool>> second,
        Func<bool, bool> predicate,
        bool yields)
    {
        var expected =
            from a in maybe
            from b in first(a)
            from c in second(b)
            where predicate(c)
            let label = $"{a}/{b}"
            select $"{label}/{c}";
        Func<string, Task<Maybe<bool>>> asyncSecond = value => Deferred.Lift(second(value), yields);

        var actual = await Sources.Apply(
            maybe,
            kind,
            s =>
                from a in s
                from b in first(a)
                from c in asyncSecond(b)
                where predicate(c)
                let label = $"{a}/{b}"
                select $"{label}/{c}",
            s =>
                from a in s
                from b in first(a)
                from c in asyncSecond(b)
                where predicate(c)
                let label = $"{a}/{b}"
                select $"{label}/{c}");

        Assert.Equal(expected, actual);
    }

    [Property]
    public async Task A_query_can_start_from_a_plain_maybe(
        Maybe<int> maybe,
        Func<int, Maybe<string>> first,
        Func<string, Maybe<bool>> second,
        bool yields)
    {
        var expected =
            from a in maybe
            from b in first(a)
            from c in second(b)
            select $"{a}/{b}/{c}";

        var asyncFirst =
            from a in maybe
            from b in Deferred.Lift(first(a), yields)
            from c in second(b)
            select $"{a}/{b}/{c}";
        var asyncSecond =
            from a in maybe
            from b in first(a)
            from c in Deferred.Lift(second(b), yields)
            select $"{a}/{b}/{c}";

        Assert.Equal(expected, await asyncFirst);
        Assert.Equal(expected, await asyncSecond);
    }

    [Property]
    public async Task Nothing_runs_after_a_source_without_a_value(SourceKind kind)
    {
        var calls = 0;

        var actual = await Sources.Apply(
            Maybes.Empty<int>(),
            kind,
            s => from a in s where Keep(a) from b in Step(a) from c in AsyncStep(b) select Combine(a, b, c),
            s => from a in s where Keep(a) from b in Step(a) from c in AsyncStep(b) select Combine(a, b, c));

        MaybeAssert.IsNone(actual);
        Assert.Equal(0, calls);

        bool Keep(int value)
        {
            calls++;
            return true;
        }

        Maybe<string> Step(int value)
        {
            calls++;
            return $"{value}";
        }

        Task<Maybe<string>> AsyncStep(string text)
        {
            calls++;
            return Task.FromResult<Maybe<string>>(text);
        }

        string Combine(int a, string b, string c)
        {
            calls++;
            return $"{a}{b}{c}";
        }
    }

    [Property]
    public async Task Nothing_runs_after_a_step_without_a_value(int value, SourceKind kind, bool yields)
    {
        var calls = 0;
        Func<int, Task<Maybe<string>>> missing = _ => Deferred.Lift(Maybes.Empty<string>(), yields);
        Func<string, Task<Maybe<string>>> next = text =>
        {
            calls++;
            return Task.FromResult<Maybe<string>>(text);
        };

        var actual = await Sources.Apply(
            Maybes.Some(value),
            kind,
            s => from a in s from b in missing(a) from c in next(b) select $"{a}{b}{c}",
            s => from a in s from b in missing(a) from c in next(b) select $"{a}{b}{c}");

        MaybeAssert.IsNone(actual);
        Assert.Equal(0, calls);
    }

    [Property]
    public async Task Every_step_runs_once_for_a_value(int value, SourceKind kind, bool yields)
    {
        // keep, step, asyncStep, combine
        var calls = new int[4];

        var actual = await Sources.Apply(
            Maybes.Some(value),
            kind,
            s => from a in s where Keep(a) from b in Step(a) from c in AsyncStep(b) select Combine(a, b, c),
            s => from a in s where Keep(a) from b in Step(a) from c in AsyncStep(b) select Combine(a, b, c));

        Assert.Equal(Maybes.Some($"{value}|{value}|{value}"), actual);
        int[] once = [1, 1, 1, 1];
        Assert.Equal(once, calls);

        bool Keep(int number)
        {
            calls[0]++;
            return true;
        }

        Maybe<string> Step(int number)
        {
            calls[1]++;
            return $"{number}";
        }

        Task<Maybe<string>> AsyncStep(string text)
        {
            calls[2]++;
            return Deferred.Lift<Maybe<string>>(text, yields);
        }

        string Combine(int a, string b, string c)
        {
            calls[3]++;
            return $"{a}|{b}|{c}";
        }
    }

    [Property]
    public void A_completed_source_with_completed_steps_is_already_complete(
        Maybe<int> maybe,
        Func<int, string> map,
        Func<int, bool> predicate,
        Func<int, Maybe<string>> next)
    {
        Assert.True((from a in Task.FromResult(maybe) select map(a)).IsCompletedSuccessfully);
        Assert.True((from a in ValueTask.FromResult(maybe) where predicate(a) select a).IsCompletedSuccessfully);
        Assert.True((from a in Task.FromResult(maybe) from b in next(a) select b).IsCompletedSuccessfully);
        Assert.True((from a in ValueTask.FromResult(maybe) from b in next(a) select b).IsCompletedSuccessfully);
        Assert.True((from a in maybe from b in Task.FromResult(next(a)) select b).IsCompletedSuccessfully);
        Assert.True((from a in Task.FromResult(maybe) from b in Task.FromResult(next(a)) select b)
            .IsCompletedSuccessfully);
        Assert.True((from a in ValueTask.FromResult(maybe) from b in Task.FromResult(next(a)) select b)
            .IsCompletedSuccessfully);
    }

    [Fact]
    public async Task A_query_that_selects_null_gives_None()
    {
        string missing = null!;
        var one = Task.FromResult(Maybes.Some(1));
        var two = Task.FromResult(Maybes.Some(2));

        MaybeAssert.IsNone(await (from a in one select missing));
        MaybeAssert.IsNone(await (from a in one from b in two select missing));
        MaybeAssert.IsNone(await (from a in Maybes.Some(1) from b in two select missing));
    }

    [Fact]
    public async Task A_failed_source_fails_the_query_and_runs_nothing()
    {
        var calls = 0;
        Func<int, Maybe<int>> step = value => ++calls + value;
        var failure = new InvalidOperationException("source");

        var fromTask = from a in Task.FromException<Maybe<int>>(failure) from b in step(a) select a + b;
        var fromValueTask = from a in ValueTask.FromException<Maybe<int>>(failure) where a > 0 select a;

        Assert.Same(failure, await Assert.ThrowsAsync<InvalidOperationException>(async () => await fromTask));
        Assert.Same(failure, await Assert.ThrowsAsync<InvalidOperationException>(async () => await fromValueTask));
        Assert.Equal(0, calls);
    }

    [Fact]
    public async Task A_failed_step_fails_the_query()
    {
        var failure = new InvalidOperationException("step");

        var fromTask =
            from a in Task.FromResult(Maybes.Some(1))
            from b in Task.FromException<Maybe<int>>(failure)
            select a + b;
        var fromMaybe =
            from a in Maybes.Some(1)
            from b in Task.FromException<Maybe<int>>(failure)
            select a + b;

        Assert.Same(failure, await Assert.ThrowsAsync<InvalidOperationException>(async () => await fromTask));
        Assert.Same(failure, await Assert.ThrowsAsync<InvalidOperationException>(async () => await fromMaybe));
    }

    [Fact]
    public async Task A_canceled_source_cancels_the_query_and_runs_nothing()
    {
        using var cancellation = new CancellationTokenSource();
        await cancellation.CancelAsync();
        var calls = 0;
        Func<int, Maybe<int>> step = value => ++calls + value;

        var fromTask = from a in Task.FromCanceled<Maybe<int>>(cancellation.Token) from b in step(a) select a + b;
        var fromValueTask = from a in ValueTask.FromCanceled<Maybe<int>>(cancellation.Token) select a + 1;

        Assert.True(fromTask.IsCanceled);
        Assert.True(fromValueTask.IsCanceled);
        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () => await fromTask);
        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () => await fromValueTask);
        Assert.Equal(0, calls);
    }

    [Fact]
    public async Task A_throwing_step_fails_only_when_awaited()
    {
        Func<int, int> fail = _ => throw new InvalidOperationException("step");
        Func<int, bool> failToDecide = _ => throw new InvalidOperationException("step");
        Func<int, Maybe<int>> failToStep = _ => throw new InvalidOperationException("step");
        Func<int, Task<Maybe<int>>> failToStart = _ => throw new InvalidOperationException("step");
        var one = Task.FromResult(Maybes.Some(1));
        var two = Task.FromResult(Maybes.Some(2));

        await AssertFailsOnlyWhenAwaited(() => from a in one select fail(a));
        await AssertFailsOnlyWhenAwaited(() => from a in ValueTask.FromResult(Maybes.Some(1)) select fail(a));
        await AssertFailsOnlyWhenAwaited(() => from a in one where failToDecide(a) select a);
        await AssertFailsOnlyWhenAwaited(() => from a in one from b in failToStep(a) select b);
        await AssertFailsOnlyWhenAwaited(() => from a in one from b in failToStart(a) select b);
        await AssertFailsOnlyWhenAwaited(() => from a in one from b in two select fail(a + b));
        await AssertFailsOnlyWhenAwaited(() => from a in Maybes.Some(1) from b in failToStart(a) select b);
        await AssertFailsOnlyWhenAwaited(() => from a in Maybes.Some(1) from b in two select fail(a + b));
    }

    [Fact]
    public async Task A_step_canceled_by_its_token_cancels_the_query()
    {
        using var cancellation = new CancellationTokenSource();
        await cancellation.CancelAsync();
        Func<int, Maybe<int>> step = value =>
        {
            cancellation.Token.ThrowIfCancellationRequested();
            return value;
        };
        Func<int, Task<Maybe<int>>> asyncStep = value =>
        {
            cancellation.Token.ThrowIfCancellationRequested();
            return Task.FromResult<Maybe<int>>(value);
        };
        Func<int, Task<Maybe<int>>> canceledStep = _ => Task.FromCanceled<Maybe<int>>(cancellation.Token);
        var one = Task.FromResult(Maybes.Some(1));

        await AssertCanceled(from a in one from b in step(a) select b);
        await AssertCanceled(from a in one from b in asyncStep(a) select b);
        await AssertCanceled(from a in one from b in canceledStep(a) select b);
        await AssertCanceled(from a in Maybes.Some(1) from b in asyncStep(a) select b);
        await AssertCanceled(from a in Maybes.Some(1) from b in canceledStep(a) select b);
    }

    [Fact]
    public void A_missing_task_source_is_rejected_immediately()
    {
        Task<Maybe<int>> source = null!;

        AssertRejects("source", () => source.Select(value => value));
        AssertRejects("source", () => source.Where(_ => true));
        AssertRejects("source", () => source.SelectMany(value => Maybes.Some(value), (a, b) => a + b));
        AssertRejects("source", () => source.SelectMany(value => Task.FromResult(Maybes.Some(value)), (a, b) => a + b));
    }

    [Fact]
    public void Missing_delegates_are_rejected_immediately_on_task_sources()
    {
        var numbers = Task.FromResult(Maybes.Some(1));

        AssertRejects("selector", () => numbers.Select((Func<int, int>)null!));
        AssertRejects("predicate", () => numbers.Where((Func<int, bool>)null!));
        AssertRejects("selector", () => numbers.SelectMany((Func<int, Maybe<int>>)null!, (a, b) => a + b));
        AssertRejects("selector", () => numbers.SelectMany((Func<int, Task<Maybe<int>>>)null!, (a, b) => a + b));
        AssertRejects(
            "resultSelector",
            () => numbers.SelectMany(a => Maybes.Some(a), (Func<int, int, int>)null!));
        AssertRejects(
            "resultSelector",
            () => numbers.SelectMany(a => Task.FromResult(Maybes.Some(a)), (Func<int, int, int>)null!));
    }

    [Fact]
    public void Missing_delegates_are_rejected_immediately_on_value_task_sources()
    {
        var numbers = ValueTask.FromResult(Maybes.Some(1));

        AssertRejects("selector", () => numbers.Select((Func<int, int>)null!));
        AssertRejects("predicate", () => numbers.Where((Func<int, bool>)null!));
        AssertRejects("selector", () => numbers.SelectMany((Func<int, Maybe<int>>)null!, (a, b) => a + b));
        AssertRejects("selector", () => numbers.SelectMany((Func<int, Task<Maybe<int>>>)null!, (a, b) => a + b));
        AssertRejects(
            "resultSelector",
            () => numbers.SelectMany(a => Maybes.Some(a), (Func<int, int, int>)null!));
        AssertRejects(
            "resultSelector",
            () => numbers.SelectMany(a => Task.FromResult(Maybes.Some(a)), (Func<int, int, int>)null!));
    }

    [Property]
    public void Missing_delegates_are_rejected_immediately_on_plain_maybes(Maybe<int> maybe)
    {
        // D30: rejected even when the maybe is None and the delegates would never run.
        AssertRejects("selector", () => maybe.SelectMany((Func<int, Task<Maybe<int>>>)null!, (a, b) => a + b));
        AssertRejects(
            "resultSelector",
            () => maybe.SelectMany(a => Task.FromResult(Maybes.Some(a)), (Func<int, int, int>)null!));
    }

    [Fact]
    public async Task Reads_like_the_readme_example()
    {
        static Task<Maybe<string>> FindUserAsync(int id) =>
            Deferred.Lift<Maybe<string>>(id > 0 ? $"user {id}" : none, yields: true);

        static Task<Maybe<decimal>> LastOrderTotalAsync(string user) =>
            Deferred.Lift<Maybe<decimal>>(user.EndsWith('0') ? none : user.Length * 10m, yields: true);

        static ValueTask<string> Report(int id) =>
            (from user in FindUserAsync(id)
             from total in LastOrderTotalAsync(user)
             where total > 0
             select $"{user}: {total}")
            .OrDefaultAsync("no orders");

        Assert.Equal("user 7: 60", await Report(7));
        Assert.Equal("no orders", await Report(10));
        Assert.Equal("no orders", await Report(-1));
    }

    private static async Task AssertFailsOnlyWhenAwaited(Func<ValueTask<Maybe<int>>> query)
    {
        ValueTask<Maybe<int>> pending = default;
        var thrownByTheCall = Record.Exception(() => { pending = query(); });

        Assert.Null(thrownByTheCall);
        var error = await Assert.ThrowsAsync<InvalidOperationException>(async () => await pending);
        Assert.Equal("step", error.Message);
    }

    private static async Task AssertCanceled(ValueTask<Maybe<int>> query)
    {
        Assert.True(query.IsCanceled);
        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () => await query);
    }

    private static void AssertRejects(string parameter, Func<object> call) =>
        Assert.Equal(parameter, Assert.Throws<ArgumentNullException>(call).ParamName);
}
