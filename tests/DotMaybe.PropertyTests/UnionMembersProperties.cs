using FsCheck.Xunit;
using static DotMaybe.Prelude;

namespace DotMaybe.PropertyTests;

/// <summary>
/// The behavioral rules the C# compiler assumes for a custom union type, checked through the (public, but not
/// intended for direct use) <c>IUnionMembers</c> interface:
/// soundness (<c>Value</c> is always one of the case types), stability (<c>Value</c> is what went in) and
/// access-pattern consistency (<c>TryGetValue</c> agrees with <c>Value</c>).
/// Maybe tightens soundness: <c>Value</c> is never <see langword="null"/>, not even for <c>default</c>.
/// </summary>
public sealed class UnionMembersProperties
{
    [Property]
    public void Value_is_never_null(Maybe<int> maybe)
    {
        Maybe<int>.IUnionMembers members = maybe;

        Assert.NotNull(members.Value);
    }

    [Property]
    public void Value_is_never_null_for_reference_values(Maybe<string> maybe)
    {
        Maybe<string>.IUnionMembers members = maybe;

        Assert.NotNull(members.Value);
    }

    [Property]
    public void Value_is_either_the_value_or_None(Maybe<int> maybe)
    {
        Maybe<int>.IUnionMembers members = maybe;

        Assert.True(
            members.Value is int or None,
            $"Expected Value to be an int or None, but it is {members.Value?.GetType().Name ?? "null"}.");
    }

    [Property]
    public void Value_of_a_converted_value_is_that_value(int value)
    {
        Maybe<int>.IUnionMembers members = Maybes.Some(value);

        Assert.Equal<object>(value, members.Value);
    }

    [Fact]
    public void Value_of_none_is_None()
    {
        Maybe<int>.IUnionMembers members = Maybes.Empty<int>();

        Assert.IsType<None>(members.Value);
    }

    [Fact]
    public void Value_of_default_is_None()
    {
        Maybe<int>.IUnionMembers number = default(Maybe<int>);
        Maybe<string>.IUnionMembers text = default(Maybe<string>);

        Assert.IsType<None>(number.Value);
        Assert.IsType<None>(text.Value);
    }

    [Fact]
    public void Creating_from_null_gives_None()
    {
        var maybe = Maybe<string>.IUnionMembers.Create(null!);
        Maybe<string>.IUnionMembers members = maybe;

        Assert.IsType<None>(members.Value);
    }

    [Property]
    public void TryGetValue_for_the_value_agrees_with_Value(Maybe<int> maybe)
    {
        Maybe<int>.IUnionMembers members = maybe;

        var found = members.TryGetValue(out int value);

        Assert.Equal(members.Value is int, found);
        if (found)
        {
            Assert.Equal<object>(members.Value, value);
        }
    }

    [Property]
    public void TryGetValue_for_None_agrees_with_Value(Maybe<int> maybe)
    {
        Maybe<int>.IUnionMembers members = maybe;

        var found = members.TryGetValue(out None _);

        Assert.Equal(members.Value is None, found);
    }

    [Property]
    public void Exactly_one_TryGetValue_succeeds(Maybe<string> maybe)
    {
        Maybe<string>.IUnionMembers members = maybe;

        var isValue = members.TryGetValue(out string _);
        var isNone = members.TryGetValue(out None _);

        Assert.NotEqual(isValue, isNone);
    }

    [Property]
    public void Create_agrees_with_the_conversion(int value)
    {
        var created = Maybe<int>.IUnionMembers.Create(value);
        Maybe<int> converted = value;

        Assert.Equal(converted, created);
        Assert.Equal(Maybes.Empty<int>(), Maybe<int>.IUnionMembers.Create(none));
    }
}
