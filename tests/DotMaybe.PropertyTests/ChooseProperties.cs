using FsCheck.Xunit;

namespace DotMaybe.PropertyTests;

/// <summary>
/// <c>Choose</c> keeps the values and drops the rest, in order, lazily like LINQ.
/// </summary>
public sealed class ChooseProperties
{
    [Property]
    public void Choose_keeps_the_values_in_order(Maybe<int>[] maybes)
    {
        var expected = maybes.SelectMany(maybe => maybe.Match(value => new[] { value }, () => Array.Empty<int>()));

        Assert.Equal(expected, maybes.Choose());
    }

    [Property]
    public void Choose_with_a_chooser_is_Select_then_Choose(int[] values, Func<int, Maybe<string>> chooser) =>
        Assert.Equal(values.Select(chooser).Choose(), values.Choose(chooser));

    [Property]
    public void Choosing_every_element_keeps_the_sequence(int[] values) =>
        Assert.Equal(values, values.Choose(value => Maybes.Some(value)));

    [Property]
    public void Choosing_no_element_gives_an_empty_sequence(int[] values) =>
        Assert.Empty(values.Choose(_ => Maybes.Empty<int>()));

    [Fact]
    public void Choose_is_deferred()
    {
        var maybes = new Probe<Maybe<int>>([Maybes.Some(1), Maybes.Empty<int>(), Maybes.Some(2)]);
        var numbers = new Probe<int>([1, 2, 3]);
        var calls = 0;

        var chosen = maybes.Choose();
        var odd = numbers.Choose(number =>
        {
            calls++;
            return number % 2 == 1 ? Maybes.Some(number) : Maybes.Empty<int>();
        });

        Assert.Equal(0, maybes.Pulled);
        Assert.Equal(0, numbers.Pulled);
        Assert.Equal(0, calls);
        Assert.Equal(new[] { 1, 2 }, chosen);
        Assert.Equal(new[] { 1, 3 }, odd);
        Assert.Equal(3, calls);
    }

    [Fact]
    public void Missing_arguments_are_rejected_immediately()
    {
        IEnumerable<Maybe<int>> maybes = null!;
        IEnumerable<int> numbers = null!;

        AssertRejects("source", () => maybes.Choose());
        AssertRejects("source", () => numbers.Choose(number => Maybes.Some(number)));
        AssertRejects("chooser", () => new[] { 1 }.Choose((Func<int, Maybe<int>>)null!));
    }

    private static void AssertRejects(string parameter, Func<object> call) =>
        Assert.Equal(parameter, Assert.Throws<ArgumentNullException>(call).ParamName);
}
