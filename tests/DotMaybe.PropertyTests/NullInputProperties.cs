using FsCheck.Xunit;

namespace DotMaybe.PropertyTests;

/// <summary>
/// A null that slips past nullable analysis must not create a third state: a maybe made from null is
/// indistinguishable from None through every public member, not only through pattern matching.
/// </summary>
/// <remarks>
/// These tests do not care where the null is handled (on creation or in every reader); they only pin down that
/// nothing observable tells the two apart.
/// </remarks>
public sealed class NullInputProperties
{
    public static TheoryData<string, Maybe<string>> MaybesMadeFromNull()
    {
        string text = null!;
        Maybe<string> converted = text;

        return new TheoryData<string, Maybe<string>>
        {
            { "union conversion", converted },
            { "conversion in generic code", Maybes.Some<string>(null!) },
            { "IUnionMembers.Create", Maybe<string>.IUnionMembers.Create(null!) },
        };
    }

    [Theory]
    [MemberData(nameof(MaybesMadeFromNull))]
    public void It_equals_None(string madeBy, Maybe<string> fromNull)
    {
        var empty = Maybes.Empty<string>();

        Assert.True(fromNull.Equals(empty), madeBy);
        Assert.True(empty.Equals(fromNull), madeBy);
        Assert.True(fromNull.Equals((object)empty), madeBy);
        Assert.True(fromNull == empty, madeBy);
        Assert.False(fromNull != empty, madeBy);
    }

    [Theory]
    [MemberData(nameof(MaybesMadeFromNull))]
    public void It_has_the_hash_code_of_None(string madeBy, Maybe<string> fromNull)
    {
        _ = madeBy;
        Assert.Equal(Maybes.Empty<string>().GetHashCode(), fromNull.GetHashCode());
    }

    [Theory]
    [MemberData(nameof(MaybesMadeFromNull))]
    public void It_is_shown_as_None(string madeBy, Maybe<string> fromNull)
    {
        _ = madeBy;
        Assert.Equal("None", fromNull.ToString());
    }

    [Theory]
    [MemberData(nameof(MaybesMadeFromNull))]
    public void Its_union_members_report_None(string madeBy, Maybe<string> fromNull)
    {
        Maybe<string>.IUnionMembers members = fromNull;

        Assert.IsType<None>(members.Value);
        Assert.False(members.TryGetValue(out string _), madeBy);
        Assert.True(members.TryGetValue(out None _), madeBy);
    }

    [Property]
    public void Every_maybe_compares_with_it_exactly_as_with_None(Maybe<string> other)
    {
        string text = null!;
        Maybe<string> fromNull = text;
        var empty = Maybes.Empty<string>();

        Assert.Equal(other.Equals(empty), other.Equals(fromNull));
        Assert.Equal(other == empty, other == fromNull);
    }

    [Fact]
    public void A_set_holds_it_and_None_as_one_element()
    {
        string text = null!;
        Maybe<string> fromNull = text;

        var set = new HashSet<Maybe<string>> { fromNull, Maybes.Empty<string>(), default };

        Assert.Single(set);
    }
}
