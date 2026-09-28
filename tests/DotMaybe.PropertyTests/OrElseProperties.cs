using FsCheck.Xunit;
using static DotMaybe.Prelude;

namespace DotMaybe.PropertyTests;

/// <summary>
/// <c>OrElse</c> is the alternative: the first maybe with a value wins. With <see cref="None"/> as the identity it is
/// a monoid, and the factory overload runs the alternative only when it is needed.
/// </summary>
public sealed class OrElseProperties
{
    [Property]
    public void A_value_wins_over_the_alternative(int value, Maybe<int> alternative) =>
        Assert.Equal(Maybes.Some(value), Maybes.Some(value).OrElse(alternative));

    [Property]
    public void None_gives_the_alternative(Maybe<int> alternative) =>
        Assert.Equal(alternative, Maybes.Empty<int>().OrElse(alternative));

    [Property]
    public void OrElse_is_Match_with_the_alternative(Maybe<int> maybe, Maybe<int> alternative) =>
        Assert.Equal(maybe.Match(value => Maybes.Some(value), () => alternative), maybe.OrElse(alternative));

    [Property]
    public void None_is_the_left_identity(Maybe<string> maybe) =>
        Assert.Equal(maybe, Maybes.Empty<string>().OrElse(maybe));

    [Property]
    public void None_is_the_right_identity(Maybe<string> maybe) =>
        Assert.Equal(maybe, maybe.OrElse(none));

    [Property]
    public void OrElse_is_associative(Maybe<int> a, Maybe<int> b, Maybe<int> c) =>
        Assert.Equal(a.OrElse(b).OrElse(c), a.OrElse(b.OrElse(c)));

    [Property]
    public void The_value_and_the_factory_overloads_agree(Maybe<int> maybe, Maybe<int> alternative) =>
        Assert.Equal(maybe.OrElse(alternative), maybe.OrElse(() => alternative));

    [Property]
    public void A_value_never_runs_the_alternative_factory(int value)
    {
        var calls = 0;

        _ = Maybes.Some(value).OrElse(() =>
        {
            calls++;
            return none;
        });

        Assert.Equal(0, calls);
    }

    [Property]
    public void None_runs_the_alternative_factory_exactly_once(Maybe<int> alternative)
    {
        var calls = 0;

        var result = Maybes.Empty<int>().OrElse(() =>
        {
            calls++;
            return alternative;
        });

        Assert.Equal(1, calls);
        Assert.Equal(alternative, result);
    }

    [Property]
    public void A_default_alternative_is_None(Maybe<int> maybe) =>
        Assert.Equal(maybe, maybe.OrElse(() => default));

    [Property]
    public void A_missing_alternative_factory_is_rejected_whatever_the_case(Maybe<int> maybe)
    {
        var error = Assert.Throws<ArgumentNullException>(() => maybe.OrElse((Func<Maybe<int>>)null!));

        Assert.Equal("alternative", error.ParamName);
    }

    [Fact]
    public void Reads_like_a_chain_of_sources()
    {
        static Maybe<int> Parse(string text) => int.TryParse(text, out var number) ? number : none;

        Assert.Equal(Maybes.Some(1), Parse("1").OrElse(Parse("2")).OrElse(3));
        Assert.Equal(Maybes.Some(2), Parse("x").OrElse(Parse("2")).OrElse(3));
        Assert.Equal(Maybes.Some(3), Parse("x").OrElse(() => Parse("y")).OrElse(3));
        MaybeAssert.IsNone(Parse("x").OrElse(() => Parse("y")));
    }
}
