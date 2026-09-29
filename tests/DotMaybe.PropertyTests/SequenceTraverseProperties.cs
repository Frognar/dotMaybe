using System.Globalization;
using FsCheck.Xunit;

namespace DotMaybe.PropertyTests;

/// <summary>
/// <c>Sequence</c> and <c>Traverse</c> are all-or-nothing: every value in order, or <see cref="None"/> as soon as one
/// is missing. <c>Traverse(f)</c> is <c>Select(f).Sequence()</c>.
/// </summary>
public sealed class SequenceTraverseProperties
{
    [Property]
    public void Sequence_gives_all_values_when_every_maybe_has_one(int[] values) =>
        Assert.Equal(values, Read(values.Select(value => Maybes.Some(value)).Sequence()));

    [Property]
    public void Sequence_gives_None_when_any_maybe_has_none(Maybe<int>[] before, Maybe<int>[] after) =>
        MaybeAssert.IsNone(before.Append(Maybes.Empty<int>()).Concat(after).Sequence());

    [Property]
    public void Sequence_holds_what_Choose_keeps_when_nothing_is_missing(Maybe<int>[] maybes)
    {
        var complete = maybes.All(maybe => maybe.Match(_ => true, () => false));

        Assert.Equal(complete ? maybes.Choose().ToArray() : null, Read(maybes.Sequence()));
    }

    [Fact]
    public void An_empty_sequence_gives_an_empty_list()
    {
        Assert.Equal(Array.Empty<int>(), Read(Array.Empty<Maybe<int>>().Sequence()));
        Assert.Equal(Array.Empty<string>(), Read(Array.Empty<int>().Traverse(_ => Maybes.Some("never"))));
    }

    [Fact]
    public void Sequence_stops_reading_at_the_first_missing_value()
    {
        var maybes = new Probe<Maybe<int>>([Maybes.Some(1), Maybes.Empty<int>(), Maybes.Some(2), Maybes.Some(3)]);

        MaybeAssert.IsNone(maybes.Sequence());
        Assert.Equal(2, maybes.Pulled);
    }

    [Property]
    public void Traverse_is_Select_then_Sequence(int[] values, Func<int, Maybe<string>> selector) =>
        Assert.Equal(Read(values.Select(selector).Sequence()), Read(values.Traverse(selector)));

    [Property]
    public void Sequence_is_Traverse_with_the_identity(Maybe<int>[] maybes) =>
        Assert.Equal(Read(maybes.Traverse(maybe => maybe)), Read(maybes.Sequence()));

    [Fact]
    public void Traverse_stops_calling_the_selector_at_the_first_missing_value()
    {
        var calls = 0;

        var result = new[] { 1, 2, 3, 4 }.Traverse(number =>
        {
            calls++;
            return number == 2 ? Maybes.Empty<int>() : Maybes.Some(number);
        });

        MaybeAssert.IsNone(result);
        Assert.Equal(2, calls);
    }

    [Fact]
    public void The_result_is_a_snapshot_of_the_source()
    {
        var maybes = new List<Maybe<int>> { Maybes.Some(1) };
        var numbers = new List<int> { 1 };

        var sequenced = maybes.Sequence();
        var traversed = numbers.Traverse(number => Maybes.Some(number));
        maybes.Add(Maybes.Some(2));
        numbers.Add(2);

        Assert.Equal(new[] { 1 }, Read(sequenced));
        Assert.Equal(new[] { 1 }, Read(traversed));
    }

    [Fact]
    public void Reads_like_parsing_all_or_nothing()
    {
        static Maybe<IReadOnlyList<int>> ParseAll(params string[] texts) =>
            texts.Traverse(text => Maybe.Parse<int>(text, CultureInfo.InvariantCulture));

        Assert.Equal(new[] { 1, 2, 3 }, Read(ParseAll("1", "2", "3")));
        MaybeAssert.IsNone(ParseAll("1", "two", "3"));
    }

    [Fact]
    public void Missing_arguments_are_rejected_immediately()
    {
        IEnumerable<Maybe<int>> maybes = null!;
        IEnumerable<int> numbers = null!;

        AssertRejects("source", () => maybes.Sequence());
        AssertRejects("source", () => numbers.Traverse(number => Maybes.Some(number)));
        AssertRejects("selector", () => new[] { 1 }.Traverse((Func<int, Maybe<int>>)null!));
    }

    private static T[]? Read<T>(Maybe<IReadOnlyList<T>> maybe) =>
        maybe.Match<T[]?>(values => values.ToArray(), () => null);

    private static void AssertRejects(string parameter, Func<object> call) =>
        Assert.Equal(parameter, Assert.Throws<ArgumentNullException>(call).ParamName);
}
