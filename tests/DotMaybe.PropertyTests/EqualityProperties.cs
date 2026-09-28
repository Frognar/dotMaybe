using FsCheck.Xunit;

namespace DotMaybe.PropertyTests;

/// <summary>
/// Structural equality: two maybes are equal when both are None, or when both hold equal values.
/// Maybe&lt;bool&gt; has only three values (None, true, false), so random pairs and triples collide often
/// and the equivalence laws get exercised on equal as well as unequal inputs.
/// </summary>
public sealed class EqualityProperties
{
    [Property]
    public void Equality_is_reflexive(Maybe<int> maybe)
    {
        var same = maybe;

        Assert.True(maybe.Equals(same));
        Assert.True(maybe == same);
        Assert.False(maybe != same);
    }

    [Property]
    public void Equality_is_symmetric(Maybe<bool> a, Maybe<bool> b) =>
        Assert.Equal(a.Equals(b), b.Equals(a));

    [Property]
    public void Equality_is_transitive(Maybe<bool> a, Maybe<bool> b, Maybe<bool> c)
    {
        if (a.Equals(b) && b.Equals(c))
        {
            Assert.True(a.Equals(c), $"{Maybes.Describe(a)} = {Maybes.Describe(b)} = {Maybes.Describe(c)}");
        }
    }

    [Property]
    public void Values_are_equal_exactly_when_their_contents_are_equal(bool x, bool y) =>
        Assert.Equal(x == y, Maybes.Some(x).Equals(Maybes.Some(y)));

    [Property]
    public void Numbers_are_equal_exactly_when_their_contents_are_equal(int x, int y) =>
        Assert.Equal(x == y, Maybes.Some(x).Equals(Maybes.Some(y)));

    [Property]
    public void Equality_uses_the_value_equality_of_the_content(string value)
    {
        // A copy is a different string instance with the same characters.
        var copy = new string(value.AsSpan());

        Assert.True(Maybes.Some(value).Equals(Maybes.Some(copy)));
        Assert.True(Maybes.Some(value) == Maybes.Some(copy));
    }

    [Property]
    public void A_value_never_equals_None(int value)
    {
        var some = Maybes.Some(value);
        var empty = Maybes.Empty<int>();

        Assert.False(some.Equals(empty));
        Assert.False(empty.Equals(some));
        Assert.False(some == empty);
        Assert.True(some != empty);
    }

    [Fact]
    public void None_equals_None_however_it_was_made()
    {
        var fromNone = Maybes.Empty<int>();
        var fromDefault = default(Maybe<int>);

        Assert.True(fromNone.Equals(fromDefault));
        Assert.True(fromNone == fromDefault);
        Assert.Equal(fromNone.GetHashCode(), fromDefault.GetHashCode());
    }

    [Property]
    public void A_value_holding_None_is_not_None(Maybe<int> inner)
    {
        // Some(None) and None are different values of Maybe<Maybe<int>>.
        var outer = Maybes.Some(inner);

        Assert.False(outer.Equals(Maybes.Empty<Maybe<int>>()));
    }

    [Property]
    public void Operators_agree_with_Equals(Maybe<bool> a, Maybe<bool> b)
    {
        Assert.Equal(a.Equals(b), a == b);
        Assert.Equal(!a.Equals(b), a != b);
    }

    [Property]
    public void Object_Equals_agrees_with_typed_Equals(Maybe<bool> a, Maybe<bool> b) =>
        Assert.Equal(a.Equals(b), a.Equals((object)b));

    [Property]
    public void Object_Equals_rejects_everything_that_is_not_the_same_maybe_type(int value)
    {
        var maybe = Maybes.Some(value);

        Assert.False(maybe.Equals((object?)null));
        Assert.False(maybe.Equals((object)value));
        Assert.False(maybe.Equals((object)Maybes.Some((long)value)));
        Assert.False(maybe.Equals((object)$"{value}"));
    }

    [Property]
    public void Equal_maybes_have_equal_hash_codes(Maybe<bool> a, Maybe<bool> b)
    {
        if (a.Equals(b))
        {
            Assert.Equal(a.GetHashCode(), b.GetHashCode());
        }
    }

    [Property]
    public void Equal_contents_give_equal_hash_codes(string value)
    {
        var copy = new string(value.AsSpan());

        Assert.Equal(Maybes.Some(value).GetHashCode(), Maybes.Some(copy).GetHashCode());
    }

    [Fact]
    public void Maybes_work_as_set_elements()
    {
        var set = new HashSet<Maybe<bool>>
        {
            Maybes.Empty<bool>(),
            default,
            Maybes.Some(true),
            Maybes.Some(true),
            Maybes.Some(false),
        };

        Assert.Equal(3, set.Count);
        Assert.Contains(Maybes.Empty<bool>(), set);
        Assert.Contains(Maybes.Some(true), set);
        Assert.Contains(Maybes.Some(false), set);
    }
}
