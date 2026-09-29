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

    [Theory]
    [MemberData(nameof(MaybesMadeFromNull))]
    public void Match_takes_the_none_branch(string madeBy, Maybe<string> fromNull)
    {
        _ = madeBy;
        Assert.Equal("none", fromNull.Match(_ => "some", () => "none"));
    }

    [Theory]
    [MemberData(nameof(MaybesMadeFromNull))]
    public void OrDefault_gives_the_fallback(string madeBy, Maybe<string> fromNull)
    {
        _ = madeBy;
        Assert.Equal("fallback", fromNull.OrDefault("fallback"));
        Assert.Equal("fallback", fromNull.OrDefault(() => "fallback"));
    }

    [Theory]
    [MemberData(nameof(MaybesMadeFromNull))]
    public void Map_Bind_and_Filter_skip_it(string madeBy, Maybe<string> fromNull)
    {
        var calls = 0;

        var mapped = fromNull.Map(text =>
        {
            calls++;
            return text.Length;
        });
        var bound = fromNull.Bind(text =>
        {
            calls++;
            return Maybes.Some(text.Length);
        });
        var filtered = fromNull.Filter(_ =>
        {
            calls++;
            return true;
        });

        Assert.Equal(0, calls);
        Assert.True(mapped == Maybes.Empty<int>(), madeBy);
        Assert.True(bound == Maybes.Empty<int>(), madeBy);
        Assert.True(filtered == Maybes.Empty<string>(), madeBy);
    }

    [Theory]
    [MemberData(nameof(MaybesMadeFromNull))]
    public void Queries_over_it_give_None(string madeBy, Maybe<string> fromNull)
    {
        var selected =
            from text in fromNull
            select text.Length;

        var joined =
            from text in fromNull
            from other in Maybes.Some("other")
            select text + other;

        Assert.True(selected == Maybes.Empty<int>(), madeBy);
        Assert.True(joined == Maybes.Empty<string>(), madeBy);
    }

    [Theory]
    [MemberData(nameof(MaybesMadeFromNull))]
    public void Combinators_treat_it_as_None(string madeBy, Maybe<string> fromNull)
    {
        var calls = 0;
        var nested = Maybes.Some(fromNull);

        var orElse = fromNull.OrElse(Maybes.Some("alternative"));
        var zipped = fromNull.Zip(Maybes.Some(1), (text, number) =>
        {
            calls++;
            return text + number;
        });
        var folded = fromNull.Fold("state", (state, text) =>
        {
            calls++;
            return state + text;
        });
        fromNull.Iter(_ => calls++);
        var tapped = fromNull.Tap(_ => calls++);

        Assert.Equal(0, calls);
        Assert.Equal(Maybes.Some("alternative"), orElse);
        Assert.True(zipped == Maybes.Empty<string>(), madeBy);
        Assert.Equal("state", folded);
        Assert.True(nested.Flatten() == Maybes.Empty<string>(), madeBy);
        Assert.True(tapped == Maybes.Empty<string>(), madeBy);
    }

    [Theory]
    [MemberData(nameof(MaybesMadeFromNull))]
    public void Collections_and_nullables_treat_it_as_None(string madeBy, Maybe<string> fromNull)
    {
        Maybe<string>[] maybes = [fromNull, Maybes.Some("value")];

        Assert.Equal(new[] { "value" }, maybes.Choose());
        Assert.True(new[] { fromNull }.Sequence().Match(_ => false, () => true), madeBy);
        Assert.Null(fromNull.ToNullable());
    }

    [Theory]
    [MemberData(nameof(MaybesMadeFromNull))]
    public async Task Async_queries_over_it_give_None_without_waiting(string madeBy, Maybe<string> fromNull)
    {
        var calls = 0;
        Func<string, Task<Maybe<string>>> next = text =>
        {
            calls++;
            return Task.FromResult(Maybes.Some(text));
        };

        var fromTask =
            from text in Task.FromResult(fromNull)
            from other in next(text)
            select text + other;
        var fromMaybe =
            from text in fromNull
            from other in next(text)
            select text + other;

        Assert.True(fromTask.IsCompletedSuccessfully, madeBy);
        Assert.True(fromMaybe.IsCompletedSuccessfully, madeBy);
        Assert.True(await fromTask == Maybes.Empty<string>(), madeBy);
        Assert.True(await fromMaybe == Maybes.Empty<string>(), madeBy);
        Assert.Equal(0, calls);
    }

    [Theory]
    [MemberData(nameof(MaybesMadeFromNull))]
    public async Task Async_steps_skip_it_without_waiting(string madeBy, Maybe<string> fromNull)
    {
        var calls = 0;

        var mapped = fromNull.MapAsync(text =>
        {
            calls++;
            return Task.FromResult(text.Length);
        });
        var matched = fromNull.MatchAsync(
            text =>
            {
                calls++;
                return Task.FromResult(text);
            },
            () => "none");

        Assert.True(mapped.IsCompletedSuccessfully, madeBy);
        Assert.True(matched.IsCompletedSuccessfully, madeBy);
        Assert.True(await mapped == Maybes.Empty<int>(), madeBy);
        Assert.Equal("none", await matched);
        Assert.Equal(0, calls);
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
