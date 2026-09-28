using FsCheck.Xunit;

namespace DotMaybe.PropertyTests;

/// <summary>
/// <c>Zip</c> combines two independent maybes: a value only when both have one. It is the applicative
/// combination, so it agrees with <c>Bind</c> + <c>Map</c> and with a two-clause query.
/// </summary>
public sealed class ZipProperties
{
    [Property]
    public void Two_values_zip_into_a_pair(int first, bool second) =>
        Assert.Equal(Maybes.Some((first, second)), Maybes.Some(first).Zip(Maybes.Some(second)));

    [Property]
    public void None_on_the_left_gives_None(Maybe<string> other) =>
        MaybeAssert.IsNone(Maybes.Empty<int>().Zip(other));

    [Property]
    public void None_on_the_right_gives_None(Maybe<int> maybe) =>
        MaybeAssert.IsNone(maybe.Zip(Maybes.Empty<string>()));

    [Property]
    public void Zip_is_Bind_then_Map(Maybe<int> maybe, Maybe<string> other) =>
        Assert.Equal(maybe.Bind(first => other.Map(second => (first, second))), maybe.Zip(other));

    [Property]
    public void Zip_with_combine_is_Zip_then_Map(
        Maybe<int> maybe,
        Maybe<string> other,
        Func<int, string, bool> combine) =>
        Assert.Equal(
            maybe.Zip(other).Map(pair => combine(pair.Item1, pair.Item2)),
            maybe.Zip(other, combine));

    [Property]
    public void Zip_agrees_with_a_two_clause_query(Maybe<int> maybe, Maybe<string> other, Func<int, string, bool> combine)
    {
        var query =
            from first in maybe
            from second in other
            select combine(first, second);

        Assert.Equal(query, maybe.Zip(other, combine));
    }

    [Property]
    public void Swapping_the_sides_swaps_the_pair(Maybe<int> maybe, Maybe<string> other) =>
        Assert.Equal(other.Zip(maybe), maybe.Zip(other).Map(pair => (pair.Item2, pair.Item1)));

    [Property]
    public void Zip_is_associative_up_to_regrouping(Maybe<int> a, Maybe<string> b, Maybe<bool> c) =>
        Assert.Equal(
            a.Zip(b).Zip(c).Map(pair => (pair.Item1.Item1, pair.Item1.Item2, pair.Item2)),
            a.Zip(b.Zip(c)).Map(pair => (pair.Item1, pair.Item2.Item1, pair.Item2.Item2)));

    [Property]
    public void Combine_runs_once_when_both_have_a_value_and_never_otherwise(Maybe<int> maybe, Maybe<string> other)
    {
        var calls = 0;

        _ = maybe.Zip(other, (first, second) =>
        {
            calls++;
            return $"{first}{second}";
        });

        var both = maybe.Match(_ => true, () => false) && other.Match(_ => true, () => false);
        Assert.Equal(both ? 1 : 0, calls);
    }

    [Property]
    public void A_null_result_becomes_None(int first, string second) =>
        MaybeAssert.IsNone(Maybes.Some(first).Zip(Maybes.Some(second), (_, _) => (string)null!));

    [Property]
    public void A_missing_combine_is_rejected_whatever_the_case(Maybe<int> maybe, Maybe<string> other)
    {
        var error = Assert.Throws<ArgumentNullException>(
            () => maybe.Zip(other, (Func<int, string, bool>)null!));

        Assert.Equal("combine", error.ParamName);
    }

    [Fact]
    public void Reads_like_combining_independent_inputs()
    {
        static Maybe<int> Parse(string text) =>
            int.TryParse(text, out var number) ? number : Maybes.Empty<int>();

        Assert.Equal(Maybes.Some(12), Parse("3").Zip(Parse("4"), (width, height) => width * height));
        MaybeAssert.IsNone(Parse("3").Zip(Parse("four"), (width, height) => width * height));
        MaybeAssert.IsNone(Parse("three").Zip(Parse("4"), (width, height) => width * height));
    }
}
