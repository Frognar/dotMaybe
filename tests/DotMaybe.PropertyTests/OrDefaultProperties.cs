using FsCheck.Xunit;

namespace DotMaybe.PropertyTests;

/// <summary>
/// <c>OrDefault</c> is <c>Match</c> with the identity on the value: the value, or a fallback.
/// The factory overload runs the fallback only when it is needed.
/// </summary>
public sealed class OrDefaultProperties
{
    [Property]
    public void A_value_ignores_the_fallback(int value, int fallback) =>
        Assert.Equal(value, Maybes.Some(value).OrDefault(fallback));

    [Property]
    public void None_gives_the_fallback(int fallback) =>
        Assert.Equal(fallback, Maybes.Empty<int>().OrDefault(fallback));

    [Property]
    public void OrDefault_is_Match_with_the_identity(Maybe<int> maybe, int fallback) =>
        Assert.Equal(maybe.Match(value => value, () => fallback), maybe.OrDefault(fallback));

    [Property]
    public void OrDefault_is_Match_with_the_identity_for_reference_values(Maybe<string> maybe, string fallback) =>
        Assert.Equal(maybe.Match(value => value, () => fallback), maybe.OrDefault(fallback));

    [Property]
    public void The_value_and_the_factory_overloads_agree(Maybe<int> maybe, int fallback) =>
        Assert.Equal(maybe.OrDefault(fallback), maybe.OrDefault(() => fallback));

    [Property]
    public void A_value_never_runs_the_fallback_factory(int value)
    {
        var calls = 0;

        _ = Maybes.Some(value).OrDefault(() =>
        {
            calls++;
            return 0;
        });

        Assert.Equal(0, calls);
    }

    [Property]
    public void None_runs_the_fallback_factory_exactly_once(int fallback)
    {
        var calls = 0;

        var result = Maybes.Empty<int>().OrDefault(() =>
        {
            calls++;
            return fallback;
        });

        Assert.Equal(1, calls);
        Assert.Equal(fallback, result);
    }

    [Fact]
    public void Default_gives_the_fallback() =>
        Assert.Equal("fallback", default(Maybe<string>).OrDefault("fallback"));

    [Property]
    public void A_null_fallback_is_rejected_whatever_the_case(Maybe<string> maybe)
    {
        var error = Assert.Throws<ArgumentNullException>(() => maybe.OrDefault((string)null!));

        Assert.Equal("fallback", error.ParamName);
    }

    [Property]
    public void A_null_fallback_factory_is_rejected_whatever_the_case(Maybe<string> maybe)
    {
        var error = Assert.Throws<ArgumentNullException>(() => maybe.OrDefault((Func<string>)null!));

        Assert.Equal("fallback", error.ParamName);
    }
}
