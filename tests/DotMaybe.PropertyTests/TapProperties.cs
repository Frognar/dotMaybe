using FsCheck.Xunit;

namespace DotMaybe.PropertyTests;

/// <summary>
/// <c>Tap</c> is <c>Iter</c> that keeps the chain going: the same side effect, and the maybe comes back unchanged.
/// </summary>
public sealed class TapProperties
{
    [Property]
    public void Tap_returns_the_maybe_unchanged(Maybe<int> maybe) =>
        Assert.Equal(maybe, maybe.Tap(_ => { }));

    [Property]
    public void The_action_runs_once_with_the_value(int value)
    {
        var seen = new List<int>();

        var result = Maybes.Some(value).Tap(seen.Add);

        Assert.Equal(new[] { value }, seen);
        Assert.Equal(Maybes.Some(value), result);
    }

    [Fact]
    public void None_never_runs_the_action()
    {
        var calls = 0;

        MaybeAssert.IsNone(Maybes.Empty<int>().Tap(_ => calls++));
        MaybeAssert.IsNone(default(Maybe<int>).Tap(_ => calls++));

        Assert.Equal(0, calls);
    }

    [Property]
    public void Tap_sees_what_Iter_sees(Maybe<string> maybe)
    {
        var tapped = new List<string>();
        var iterated = new List<string>();

        _ = maybe.Tap(tapped.Add);
        maybe.Iter(iterated.Add);

        Assert.Equal(iterated, tapped);
    }

    [Property]
    public void Tapping_does_not_change_what_follows(Maybe<int> maybe, Func<int, string> map) =>
        Assert.Equal(maybe.Map(map), maybe.Tap(_ => { }).Map(map));

    [Property]
    public void Side_effects_run_in_chain_order(int value)
    {
        var log = new List<string>();

        var result = Maybes.Some(value)
            .Tap(number => log.Add($"before {number}"))
            .Map(number => number.ToString(System.Globalization.CultureInfo.InvariantCulture))
            .Tap(text => log.Add($"after {text}"));

        var text = value.ToString(System.Globalization.CultureInfo.InvariantCulture);
        Assert.Equal(new[] { $"before {value}", $"after {text}" }, log);
        Assert.Equal(Maybes.Some(text), result);
    }

    [Property]
    public void A_missing_action_is_rejected_whatever_the_case(Maybe<int> maybe)
    {
        var error = Assert.Throws<ArgumentNullException>(() => maybe.Tap(null!));

        Assert.Equal("action", error.ParamName);
    }
}
