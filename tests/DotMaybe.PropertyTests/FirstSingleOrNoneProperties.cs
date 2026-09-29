using FsCheck.Xunit;

namespace DotMaybe.PropertyTests;

/// <summary>
/// <c>FirstOrNone</c> and <c>SingleOrNone</c> are the total versions of <c>First</c> and <c>Single</c>: no exception,
/// <see cref="None"/> instead. They read no more of the sequence than they need.
/// </summary>
public sealed class FirstSingleOrNoneProperties
{
    [Property]
    public void FirstOrNone_gives_the_first_element(int[] values)
    {
        var expected = values.Length > 0 ? Maybes.Some(values[0]) : Maybes.Empty<int>();

        Assert.Equal(expected, values.FirstOrNone());
    }

    [Property]
    public void FirstOrNone_with_a_predicate_is_Where_then_FirstOrNone(int[] values, Func<int, bool> predicate) =>
        Assert.Equal(values.Where(predicate).FirstOrNone(), values.FirstOrNone(predicate));

    [Fact]
    public void FirstOrNone_reads_up_to_the_first_match_only()
    {
        var first = new Probe<int>([1, 2, 3, 4]);
        var firstEven = new Probe<int>([1, 2, 3, 4]);

        Assert.Equal(Maybes.Some(1), first.FirstOrNone());
        Assert.Equal(Maybes.Some(2), firstEven.FirstOrNone(number => number % 2 == 0));
        Assert.Equal(1, first.Pulled);
        Assert.Equal(2, firstEven.Pulled);
    }

    [Property]
    public void SingleOrNone_needs_exactly_one_element(int[] values)
    {
        var expected = values.Length == 1 ? Maybes.Some(values[0]) : Maybes.Empty<int>();

        Assert.Equal(expected, values.SingleOrNone());
    }

    [Property]
    public void SingleOrNone_with_a_predicate_is_Where_then_SingleOrNone(int[] values, Func<int, bool> predicate) =>
        Assert.Equal(values.Where(predicate).SingleOrNone(), values.SingleOrNone(predicate));

    [Fact]
    public void Several_elements_give_None_instead_of_throwing()
    {
        MaybeAssert.IsNone(new[] { 1, 2 }.SingleOrNone());
        MaybeAssert.IsNone(new[] { 1, 2, 3 }.SingleOrNone(number => number > 1));
    }

    [Fact]
    public void SingleOrNone_reads_up_to_the_second_match_only()
    {
        var all = new Probe<int>([1, 2, 3, 4]);
        var greater = new Probe<int>([1, 2, 3, 4]);

        MaybeAssert.IsNone(all.SingleOrNone());
        MaybeAssert.IsNone(greater.SingleOrNone(number => number > 1));
        Assert.Equal(2, all.Pulled);
        Assert.Equal(3, greater.Pulled);
    }

    [Fact]
    public void A_null_element_gives_None()
    {
        // D26: null never becomes a value. A null first (or only) element is found, and it is None.
        string[] texts = [null!, "text"];
        string[] single = [null!];

        MaybeAssert.IsNone(texts.FirstOrNone());
        MaybeAssert.IsNone(single.SingleOrNone());
    }

    [Fact]
    public void Missing_arguments_are_rejected_immediately()
    {
        IEnumerable<int> numbers = null!;
        var some = new[] { 1 };

        AssertRejects("source", () => numbers.FirstOrNone());
        AssertRejects("source", () => numbers.FirstOrNone(_ => true));
        AssertRejects("predicate", () => some.FirstOrNone(null!));
        AssertRejects("source", () => numbers.SingleOrNone());
        AssertRejects("source", () => numbers.SingleOrNone(_ => true));
        AssertRejects("predicate", () => some.SingleOrNone(null!));
    }

    private static void AssertRejects(string parameter, Func<object> call) =>
        Assert.Equal(parameter, Assert.Throws<ArgumentNullException>(call).ParamName);
}
