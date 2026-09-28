using FsCheck.Xunit;

namespace DotMaybe.PropertyTests;

/// <summary>
/// <c>Map</c> makes <c>Maybe</c> a functor: it transforms the value and leaves the structure alone.
/// </summary>
public sealed class MapProperties
{
    [Property]
    public void A_value_is_transformed(int value, Func<int, string> map) =>
        Assert.Equal(Maybes.Some(map(value)), Maybes.Some(value).Map(map));

    [Property]
    public void None_stays_None_and_map_never_runs(Func<int, string> map)
    {
        var calls = 0;

        var result = Maybes.Empty<int>().Map(value =>
        {
            calls++;
            return map(value);
        });

        Assert.Equal(Maybes.Empty<string>(), result);
        Assert.Equal(0, calls);
    }

    [Property]
    public void Map_runs_exactly_once_on_a_value(int value)
    {
        var calls = 0;

        _ = Maybes.Some(value).Map(number =>
        {
            calls++;
            return number;
        });

        Assert.Equal(1, calls);
    }

    [Property]
    public void Mapping_the_identity_changes_nothing(Maybe<int> maybe) =>
        Assert.Equal(maybe, maybe.Map(value => value));

    [Property]
    public void Mapping_twice_is_mapping_the_composition(
        Maybe<int> maybe,
        Func<int, string> first,
        Func<string, bool> second) =>
        Assert.Equal(maybe.Map(first).Map(second), maybe.Map(value => second(first(value))));

    [Property]
    public void Map_is_Match_that_wraps_the_result(Maybe<int> maybe, Func<int, string> map) =>
        Assert.Equal(
            maybe.Match(value => Maybes.Some(map(value)), () => Maybes.Empty<string>()),
            maybe.Map(map));

    [Property]
    public void A_null_result_becomes_None(Maybe<string> maybe) =>
        MaybeAssert.IsNone(maybe.Map<string>(_ => null!));

    [Property]
    public void Map_works_on_nested_maybes(Maybe<Maybe<int>> outer, Func<int, string> map) =>
        Assert.Equal(
            outer.Match(inner => Maybes.Some(inner.Map(map)), () => Maybes.Empty<Maybe<string>>()),
            outer.Map(inner => inner.Map(map)));

    [Property]
    public void A_missing_map_is_rejected_whatever_the_case(Maybe<int> maybe)
    {
        var error = Assert.Throws<ArgumentNullException>(() => maybe.Map<string>(null!));

        Assert.Equal("map", error.ParamName);
    }
}
