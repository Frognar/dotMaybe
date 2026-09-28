using FsCheck.Xunit;

namespace DotMaybe.PropertyTests;

/// <summary>
/// <c>Flatten</c> is the monad's join: it removes exactly one level of nesting, and <c>Map</c> followed by
/// <c>Flatten</c> is <c>Bind</c>.
/// </summary>
public sealed class FlattenProperties
{
    [Property]
    public void A_value_holding_a_maybe_gives_that_maybe(Maybe<int> inner) =>
        Assert.Equal(inner, Maybes.Some(inner).Flatten());

    [Fact]
    public void None_gives_None()
    {
        MaybeAssert.IsNone(Maybes.Empty<Maybe<int>>().Flatten());
        MaybeAssert.IsNone(default(Maybe<Maybe<int>>).Flatten());
    }

    [Fact]
    public void A_value_holding_None_and_None_flatten_alike()
    {
        var someNone = Maybes.Some(Maybes.Empty<int>());
        var noneAtAll = Maybes.Empty<Maybe<int>>();

        Assert.NotEqual(someNone, noneAtAll);
        Assert.Equal(someNone.Flatten(), noneAtAll.Flatten());
    }

    [Property]
    public void Flatten_is_Bind_with_the_identity(Maybe<Maybe<int>> outer) =>
        Assert.Equal(outer.Bind(inner => inner), outer.Flatten());

    [Property]
    public void Map_then_Flatten_is_Bind(Maybe<int> maybe, Func<int, Maybe<string>> bind) =>
        Assert.Equal(maybe.Bind(bind), maybe.Map(bind).Flatten());

    [Property]
    public void Wrapping_then_flattening_changes_nothing(Maybe<int> maybe)
    {
        Assert.Equal(maybe, Maybes.Some(maybe).Flatten());
        Assert.Equal(maybe, maybe.Map(value => Maybes.Some(value)).Flatten());
    }

    [Property]
    public void Flattening_three_levels_does_not_depend_on_the_order(Maybe<Maybe<Maybe<int>>> outer) =>
        Assert.Equal(outer.Flatten().Flatten(), outer.Map(middle => middle.Flatten()).Flatten());

    [Property]
    public void Flatten_can_be_called_as_a_static_function(Maybe<Maybe<string>> outer) =>
        Assert.Equal(outer.Flatten(), Maybe.Flatten(outer));
}
