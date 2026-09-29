using FsCheck.Xunit;

namespace DotMaybe.PropertyTests;

/// <summary>
/// Maybes are ordered like <c>Option</c> in F#: <see cref="None"/> first, then the values in their own order.
/// The order is total, consistent with equality, and what sorting and <see cref="Comparer{T}.Default"/> use.
/// </summary>
public sealed class OrderingProperties
{
    [Property]
    public void None_comes_before_every_value(int value)
    {
        Assert.True(Maybes.Empty<int>().CompareTo(Maybes.Some(value)) < 0);
        Assert.True(Maybes.Some(value).CompareTo(Maybes.Empty<int>()) > 0);
    }

    [Fact]
    public void None_is_in_the_same_position_as_None()
    {
        Assert.Equal(0, Maybes.Empty<int>().CompareTo(Maybes.Empty<int>()));
        Assert.Equal(0, default(Maybe<int>).CompareTo(Maybes.Empty<int>()));
    }

    [Property]
    public void Values_compare_like_the_values(int a, int b) =>
        Assert.Equal(Math.Sign(a.CompareTo(b)), Math.Sign(Maybes.Some(a).CompareTo(Maybes.Some(b))));

    [Property]
    public void Values_compare_like_the_values_for_other_types(Guid a, Guid b) =>
        Assert.Equal(Math.Sign(a.CompareTo(b)), Math.Sign(Maybes.Some(a).CompareTo(Maybes.Some(b))));

    [Property]
    public void The_order_is_antisymmetric(Maybe<int> x, Maybe<int> y) =>
        Assert.Equal(Math.Sign(x.CompareTo(y)), -Math.Sign(y.CompareTo(x)));

    [Property]
    public void The_order_is_transitive(Maybe<int> x, Maybe<int> y, Maybe<int> z)
    {
        if (x.CompareTo(y) <= 0 && y.CompareTo(z) <= 0)
        {
            Assert.True(x.CompareTo(z) <= 0);
        }
    }

    [Property]
    public void The_order_is_consistent_with_equality(Maybe<int> x, Maybe<int> y) =>
        Assert.Equal(x.Equals(y), x.CompareTo(y) == 0);

    [Property]
    public void Sorting_puts_None_first_then_the_values_in_order(Maybe<int>[] maybes)
    {
        var nones = maybes.Where(maybe => maybe.Match(_ => false, () => true));
        var values = maybes.Choose().Order().Select(value => Maybes.Some(value));
        var sorted = maybes.ToList();

        sorted.Sort();

        Assert.Equal(nones.Concat(values), maybes.Order());
        Assert.Equal(nones.Concat(values), sorted);
    }

    [Property]
    public void The_default_comparer_uses_CompareTo(Maybe<int> x, Maybe<int> y) =>
        Assert.Equal(Math.Sign(x.CompareTo(y)), Math.Sign(Comparer<Maybe<int>>.Default.Compare(x, y)));

    [Property]
    public void Min_and_Max_follow_the_order(Maybe<int>[] maybes)
    {
        if (maybes.Length == 0)
        {
            return;
        }

        var sorted = maybes.Order().ToArray();

        Assert.Equal(sorted[0], maybes.Min());
        Assert.Equal(sorted[^1], maybes.Max());
    }

    [Fact]
    public void Nested_maybes_put_each_None_before_the_values_it_could_hold()
    {
        var none = Maybes.Empty<Maybe<int>>();
        var someNone = Maybes.Some(Maybes.Empty<int>());
        var someOne = Maybes.Some(Maybes.Some(1));
        var someTwo = Maybes.Some(Maybes.Some(2));

        Maybe<Maybe<int>>[] shuffled = [someTwo, none, someOne, someNone];

        Assert.Equal(new[] { none, someNone, someOne, someTwo }, shuffled.Order());
    }

    [Property]
    public void The_object_overload_agrees_with_the_typed_one(Maybe<int> x, Maybe<int> y) =>
        Assert.Equal(Math.Sign(x.CompareTo(y)), Math.Sign(((IComparable)x).CompareTo(y)));

    [Property]
    public void Every_maybe_comes_after_null(Maybe<int> maybe) =>
        Assert.True(((IComparable)maybe).CompareTo(null) > 0);

    [Property]
    public void Comparing_with_something_else_is_rejected(Maybe<int> maybe)
    {
        var withNumber = Assert.Throws<ArgumentException>(() => ((IComparable)maybe).CompareTo(5));
        var withOtherMaybe = Assert.Throws<ArgumentException>(() => ((IComparable)maybe).CompareTo(Maybes.Some(5L)));

        Assert.Equal("obj", withNumber.ParamName);
        Assert.Equal("obj", withOtherMaybe.ParamName);
    }

    [Fact]
    public void Values_that_cannot_be_compared_throw_but_None_still_has_its_place()
    {
        var first = Maybes.Some(new Opaque());
        var second = Maybes.Some(new Opaque());
        var empty = Maybes.Empty<Opaque>();

        Assert.ThrowsAny<ArgumentException>(() => first.CompareTo(second));
        Assert.True(empty.CompareTo(first) < 0);
        Assert.True(first.CompareTo(empty) > 0);
        Assert.Equal(0, empty.CompareTo(empty));
    }

    private sealed class Opaque
    {
    }
}
