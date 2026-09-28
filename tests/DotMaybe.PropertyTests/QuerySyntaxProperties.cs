using FsCheck.Xunit;
using static DotMaybe.Prelude;

namespace DotMaybe.PropertyTests;

/// <summary>
/// Query expressions over maybes mean the same as the method chains they abbreviate, and the monad laws hold
/// in query form too.
/// </summary>
public sealed class QuerySyntaxProperties
{
    [Property]
    public void Select_clause_is_Map(Maybe<int> maybe, Func<int, string> map)
    {
        var query =
            from value in maybe
            select map(value);

        Assert.Equal(maybe.Map(map), query);
    }

    [Property]
    public void Two_from_clauses_are_Bind_then_Map(
        Maybe<int> maybe,
        Func<int, Maybe<string>> next,
        Func<int, string, bool> combine)
    {
        var query =
            from first in maybe
            from second in next(first)
            select combine(first, second);

        Assert.Equal(maybe.Bind(first => next(first).Map(second => combine(first, second))), query);
    }

    [Property]
    public void Where_clause_is_Filter(Maybe<int> maybe, Func<int, bool> predicate)
    {
        var query =
            from value in maybe
            where predicate(value)
            select value;

        Assert.Equal(maybe.Filter(predicate), query);
    }

    [Property]
    public void Let_clause_binds_an_intermediate_value(Maybe<int> maybe, Func<int, string> map)
    {
        var query =
            from value in maybe
            let text = map(value)
            select $"{value}:{text}";

        Assert.Equal(maybe.Map(value => $"{value}:{map(value)}"), query);
    }

    [Property]
    public void Three_from_clauses_need_all_three_values(Maybe<int> a, Maybe<int> b, Maybe<int> c)
    {
        var query =
            from x in a
            from y in b
            from z in c
            select (x, y, z);

        var expected = a is int x2 && b is int y2 && c is int z2
            ? Maybes.Some((x2, y2, z2))
            : Maybes.Empty<(int, int, int)>();

        Assert.Equal(expected, query);
    }

    [Property]
    public void Left_identity_in_query_form(int value, Func<int, Maybe<string>> next)
    {
        var query =
            from x in Maybes.Some(value)
            from y in next(x)
            select y;

        Assert.Equal(next(value), query);
    }

    [Property]
    public void Right_identity_in_query_form(Maybe<int> maybe)
    {
        var query =
            from x in maybe
            select x;

        Assert.Equal(maybe, query);
    }

    [Property]
    public void Associativity_in_query_form(
        Maybe<int> maybe,
        Func<int, Maybe<string>> first,
        Func<string, Maybe<bool>> second)
    {
        // (maybe >>= first) >>= second
        var nestedLeft =
            from b in (from a in maybe from b1 in first(a) select b1)
            from c in second(b)
            select c;

        // maybe >>= (a => first(a) >>= second)
        var nestedRight =
            from a in maybe
            from c in (from b in first(a) from c1 in second(b) select c1)
            select c;

        Assert.Equal(nestedLeft, nestedRight);
    }

    [Property]
    public void A_step_that_returns_none_stops_the_query(Maybe<int> maybe, int fallback)
    {
        var query =
            from x in maybe
            from y in Maybes.Empty<int>()
            select x + y;

        Assert.Equal(fallback, query.OrDefault(fallback));
    }

    [Fact]
    public void Reads_like_the_readme_example()
    {
        static Maybe<int> Parse(string text) => int.TryParse(text, out var number) ? number : none;

        var ratio =
            from numerator in Parse("10")
            from denominator in Parse("2")
            where denominator != 0
            select numerator / denominator;

        var divisionByZero =
            from numerator in Parse("10")
            from denominator in Parse("0")
            where denominator != 0
            select numerator / denominator;

        var notANumber =
            from numerator in Parse("ten")
            from denominator in Parse("2")
            select numerator / denominator;

        Assert.Equal(Maybes.Some(5), ratio);
        MaybeAssert.IsNone(divisionByZero);
        MaybeAssert.IsNone(notANumber);
    }
}
