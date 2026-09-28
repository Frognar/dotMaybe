using FsCheck.Xunit;

namespace DotMaybe.PropertyTests;

/// <summary>
/// <c>Match</c> is the total way out of a maybe: one branch per case, exactly one of them runs, exactly once.
/// </summary>
public sealed class MatchProperties
{
    [Property]
    public void A_value_goes_to_the_some_branch(int value, Func<int, string> some, string none) =>
        Assert.Equal(some(value), Maybes.Some(value).Match(some, () => none));

    [Property]
    public void None_goes_to_the_none_branch(Func<int, string> some, string none) =>
        Assert.Equal(none, Maybes.Empty<int>().Match(some, () => none));

    [Property]
    public void Match_agrees_with_a_switch(Maybe<int> maybe)
    {
        var bySwitch = maybe switch
        {
            int value => $"some {value}",
            None => "none",
        };

        Assert.Equal(bySwitch, maybe.Match(value => $"some {value}", () => "none"));
    }

    [Property]
    public void Exactly_one_branch_runs_exactly_once(Maybe<int> maybe)
    {
        var someCalls = 0;
        var noneCalls = 0;

        _ = maybe.Match(_ => ++someCalls, () => ++noneCalls);

        Assert.Equal(1, someCalls + noneCalls);
        Assert.Equal(maybe is int ? 1 : 0, someCalls);
    }

    [Property]
    public void Rebuilding_from_the_branches_gives_back_the_same_maybe(Maybe<int> maybe) =>
        Assert.Equal(maybe, maybe.Match(value => Maybes.Some(value), () => Maybes.Empty<int>()));

    [Property]
    public void Match_reads_values_in_generic_code(Maybe<string> maybe) =>
        Assert.Equal(Maybes.Describe(maybe), DescribeWithMatch(maybe));

    [Property]
    public void Match_reads_nested_maybes_one_level_at_a_time(Maybe<Maybe<int>> outer)
    {
        var described = outer.Match(
            inner => inner.Match(value => $"Some(Some({value}))", () => "Some(None)"),
            () => "None");

        Assert.Equal(outer.ToString(), described);
    }

    [Property]
    public void A_missing_some_branch_is_rejected_whatever_the_case(Maybe<int> maybe)
    {
        var error = Assert.Throws<ArgumentNullException>(() => maybe.Match<int>(null!, () => 0));

        Assert.Equal("some", error.ParamName);
    }

    [Property]
    public void A_missing_none_branch_is_rejected_whatever_the_case(Maybe<int> maybe)
    {
        var error = Assert.Throws<ArgumentNullException>(() => maybe.Match(value => value, null!));

        Assert.Equal("none", error.ParamName);
    }

    private static string DescribeWithMatch<T>(Maybe<T> maybe)
        where T : notnull => maybe.Match(value => $"a value ({value})", () => "None");
}
