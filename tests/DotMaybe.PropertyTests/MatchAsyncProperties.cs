using FsCheck.Xunit;

namespace DotMaybe.PropertyTests;

/// <summary>
/// <c>MatchAsync</c> and <c>OrDefaultAsync</c> behave exactly like their synchronous counterparts once awaited,
/// whether the tasks are already complete or not.
/// </summary>
public sealed class MatchAsyncProperties
{
    [Property]
    public async Task MatchAsync_agrees_with_Match(Maybe<int> maybe, Func<int, string> some, string none, bool yields) =>
        Assert.Equal(
            maybe.Match(some, () => none),
            await maybe.MatchAsync(value => Deferred.Lift(some(value), yields), () => Deferred.Lift(none, yields)));

    [Property]
    public async Task MatchAsync_with_a_synchronous_none_agrees_with_Match(
        Maybe<int> maybe,
        Func<int, string> some,
        string none,
        bool yields) =>
        Assert.Equal(
            maybe.Match(some, () => none),
            await maybe.MatchAsync(value => Deferred.Lift(some(value), yields), () => none));

    [Property]
    public async Task Exactly_one_branch_runs_exactly_once(Maybe<int> maybe, bool yields)
    {
        var someCalls = 0;
        var noneCalls = 0;

        _ = await maybe.MatchAsync(
            _ => Deferred.Lift(++someCalls, yields),
            () => Deferred.Lift(++noneCalls, yields));

        Assert.Equal(1, someCalls + noneCalls);
        Assert.Equal(maybe is int ? 1 : 0, someCalls);
    }

    [Property]
    public async Task None_with_a_synchronous_none_branch_is_already_complete(string none)
    {
        var pending = Maybes.Empty<int>().MatchAsync(value => Task.FromResult($"{value}"), () => none);

        Assert.True(pending.IsCompletedSuccessfully);
        Assert.Equal(none, await pending);
    }

    [Property]
    public async Task OrDefaultAsync_agrees_with_OrDefault(Maybe<int> maybe, int fallback, bool yields) =>
        Assert.Equal(maybe.OrDefault(fallback), await maybe.OrDefaultAsync(() => Deferred.Lift(fallback, yields)));

    [Property]
    public async Task A_value_is_already_complete_and_never_runs_the_fallback(int value)
    {
        var calls = 0;

        var pending = Maybes.Some(value).OrDefaultAsync(() =>
        {
            calls++;
            return Task.FromResult(0);
        });

        Assert.True(pending.IsCompletedSuccessfully);
        Assert.Equal(value, await pending);
        Assert.Equal(0, calls);
    }

    [Property]
    public async Task A_failing_branch_fails_the_result(Maybe<int> maybe)
    {
        var error = await Assert.ThrowsAsync<InvalidOperationException>(async () => await maybe.MatchAsync(
            _ => Task.FromException<int>(new InvalidOperationException("some")),
            () => Task.FromException<int>(new InvalidOperationException("none"))));

        Assert.Equal(maybe is int ? "some" : "none", error.Message);
    }

    [Property]
    public void Missing_branches_are_rejected_immediately_whatever_the_case(Maybe<int> maybe)
    {
        Func<int, Task<int>> some = value => Task.FromResult(value);

        Assert.Equal("some", Assert.Throws<ArgumentNullException>(
            () => maybe.MatchAsync<int>(null!, () => Task.FromResult(0))).ParamName);
        Assert.Equal("none", Assert.Throws<ArgumentNullException>(
            () => maybe.MatchAsync(some, (Func<Task<int>>)null!)).ParamName);
        Assert.Equal("some", Assert.Throws<ArgumentNullException>(
            () => maybe.MatchAsync<int>(null!, () => 0)).ParamName);
        Assert.Equal("none", Assert.Throws<ArgumentNullException>(
            () => maybe.MatchAsync(some, (Func<int>)null!)).ParamName);
    }

    [Property]
    public void A_missing_fallback_is_rejected_immediately_whatever_the_case(Maybe<int> maybe)
    {
        var error = Assert.Throws<ArgumentNullException>(() => maybe.OrDefaultAsync(null!));

        Assert.Equal("fallback", error.ParamName);
    }
}
