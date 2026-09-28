using FsCheck.Xunit;

namespace DotMaybe.PropertyTests;

/// <summary>
/// <c>Iter</c> is the one explicit place for side effects (D8): the action runs once on a value and never without one.
/// </summary>
public sealed class IterProperties
{
    [Property]
    public void The_action_runs_once_with_the_value(int value)
    {
        var seen = new List<int>();

        Maybes.Some(value).Iter(seen.Add);

        Assert.Equal(new[] { value }, seen);
    }

    [Fact]
    public void None_never_runs_the_action()
    {
        var calls = 0;

        Maybes.Empty<int>().Iter(_ => calls++);
        default(Maybe<int>).Iter(_ => calls++);

        Assert.Equal(0, calls);
    }

    [Property]
    public void Iter_sees_what_Match_sees(Maybe<string> maybe)
    {
        var seen = new List<string>();

        maybe.Iter(seen.Add);

        Assert.Equal(maybe.Match(value => new[] { value }, () => Array.Empty<string>()), seen);
    }

    [Property]
    public void A_missing_action_is_rejected_whatever_the_case(Maybe<int> maybe)
    {
        var error = Assert.Throws<ArgumentNullException>(() => maybe.Iter(null!));

        Assert.Equal("action", error.ParamName);
    }
}
