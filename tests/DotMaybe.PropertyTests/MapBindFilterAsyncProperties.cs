using FsCheck.Xunit;
using static DotMaybe.Prelude;

namespace DotMaybe.PropertyTests;

/// <summary>
/// <c>MapAsync</c>, <c>BindAsync</c> and <c>FilterAsync</c> are their synchronous counterparts once awaited.
/// The monad laws hold for asynchronous steps too. On None nothing runs and nothing waits.
/// </summary>
public sealed class MapBindFilterAsyncProperties
{
    [Property]
    public async Task MapAsync_agrees_with_Map(Maybe<int> maybe, Func<int, string> map, bool yields) =>
        Assert.Equal(maybe.Map(map), await maybe.MapAsync(value => Deferred.Lift(map(value), yields)));

    [Property]
    public async Task BindAsync_agrees_with_Bind(Maybe<int> maybe, Func<int, Maybe<string>> bind, bool yields) =>
        Assert.Equal(maybe.Bind(bind), await maybe.BindAsync(value => Deferred.Lift(bind(value), yields)));

    [Property]
    public async Task FilterAsync_agrees_with_Filter(Maybe<int> maybe, Func<int, bool> predicate, bool yields) =>
        Assert.Equal(maybe.Filter(predicate), await maybe.FilterAsync(value => Deferred.Lift(predicate(value), yields)));

    [Property]
    public async Task Left_identity(int value, Func<int, Maybe<string>> bind, bool yields) =>
        Assert.Equal(bind(value), await Maybes.Some(value).BindAsync(x => Deferred.Lift(bind(x), yields)));

    [Property]
    public async Task Right_identity(Maybe<int> maybe, bool yields) =>
        Assert.Equal(maybe, await maybe.BindAsync(value => Deferred.Lift(Maybes.Some(value), yields)));

    [Property]
    public async Task Associativity(
        Maybe<int> maybe,
        Func<int, Maybe<string>> first,
        Func<string, Maybe<bool>> second,
        bool yields)
    {
        Task<Maybe<string>> First(int value) => Deferred.Lift(first(value), yields);
        Task<Maybe<bool>> Second(string value) => Deferred.Lift(second(value), yields);

        var leftNested = await (await maybe.BindAsync(First)).BindAsync(Second);
        var rightNested = await maybe.BindAsync(async value => await (await First(value)).BindAsync(Second));

        Assert.Equal(leftNested, rightNested);
    }

    [Fact]
    public async Task None_is_already_complete_and_runs_nothing()
    {
        var calls = 0;
        var empty = Maybes.Empty<int>();

        var mapped = empty.MapAsync(value =>
        {
            calls++;
            return Task.FromResult($"{value}");
        });
        var bound = empty.BindAsync(value =>
        {
            calls++;
            return Task.FromResult(Maybes.Some(value));
        });
        var filtered = empty.FilterAsync(_ =>
        {
            calls++;
            return Task.FromResult(true);
        });

        Assert.True(mapped.IsCompletedSuccessfully);
        Assert.True(bound.IsCompletedSuccessfully);
        Assert.True(filtered.IsCompletedSuccessfully);
        MaybeAssert.IsNone(await mapped);
        MaybeAssert.IsNone(await bound);
        MaybeAssert.IsNone(await filtered);
        Assert.Equal(0, calls);
    }

    [Property]
    public async Task Each_step_runs_exactly_once_on_a_value(int value, bool yields)
    {
        var calls = 0;
        var some = Maybes.Some(value);

        _ = await some.MapAsync(number => Deferred.Lift(++calls + number, yields));
        _ = await some.BindAsync(number => Deferred.Lift(Maybes.Some(++calls + number), yields));
        _ = await some.FilterAsync(_ => Deferred.Lift(++calls > 0, yields));

        Assert.Equal(3, calls);
    }

    [Property]
    public async Task A_null_result_becomes_None(Maybe<string> maybe, bool yields) =>
        MaybeAssert.IsNone(await maybe.MapAsync(_ => Deferred.Lift<string>(null!, yields)));

    [Property]
    public async Task A_step_can_turn_a_value_into_None(Maybe<int> maybe, bool yields) =>
        MaybeAssert.IsNone(await maybe.BindAsync(_ => Deferred.Lift<Maybe<string>>(none, yields)));

    [Property]
    public async Task A_failing_step_fails_the_result(int value)
    {
        var error = await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await Maybes.Some(value).MapAsync<int>(_ => Task.FromException<int>(new InvalidOperationException("map"))));

        Assert.Equal("map", error.Message);
    }

    [Property]
    public void Missing_steps_are_rejected_immediately_whatever_the_case(Maybe<int> maybe)
    {
        Assert.Equal("map", Assert.Throws<ArgumentNullException>(() => maybe.MapAsync<string>(null!)).ParamName);
        Assert.Equal("bind", Assert.Throws<ArgumentNullException>(() => maybe.BindAsync<string>(null!)).ParamName);
        Assert.Equal("predicate", Assert.Throws<ArgumentNullException>(() => maybe.FilterAsync(null!)).ParamName);
    }
}
