namespace DotMaybe.CompilerContractTests;

/// <summary>
/// Which query expressions work over maybes: from, where, let and select do; ordering, grouping and joins do not,
/// and a query over a maybe does not silently mix with sequences.
/// </summary>
public sealed class QuerySyntaxContracts
{
    [Fact]
    public void The_readme_query_compiles()
    {
        var result = Snippet.Compile(Consumer.File("""
                private static Maybe<int> Parse(string text) => int.TryParse(text, out var number) ? number : none;

                public static Maybe<int> Ratio(string numerator, string denominator) =>
                    from a in Parse(numerator)
                    from b in Parse(denominator)
                    where b != 0
                    select a / b;
            """));

        ContractAssert.CompilesCleanly(result);
    }

    [Fact]
    public void Let_and_several_from_clauses_compile()
    {
        var result = Snippet.Compile(Consumer.File("""
                public static Maybe<string> Describe(Maybe<int> a, Maybe<int> b, Maybe<string> unit) =>
                    from x in a
                    from y in b
                    let sum = x + y
                    from u in unit
                    where sum > 0
                    select $"{sum} {u}";
            """));

        ContractAssert.CompilesCleanly(result);
    }

    [Fact]
    public void Ordering_grouping_and_joins_are_not_supported()
    {
        // Which error the compiler reports is its own detail: on SDK RC1 orderby gives CS1061 and group/join give
        // CS1936. The contract only pins that each operator is rejected by name.
        var result = Snippet.Compile(Consumer.File("""
                public static Maybe<int> Ordered(Maybe<int> maybe) =>
                    from x in maybe
                    orderby x
                    select x;

                public static object Grouped(Maybe<int> maybe) =>
                    from x in maybe
                    group x by x;

                public static Maybe<int> Joined(Maybe<int> left, Maybe<int> right) =>
                    from x in left
                    join y in right on x equals y
                    select x + y;
            """));

        ContractAssert.FailsMentioning(result, "'OrderBy'");
        ContractAssert.FailsMentioning(result, "'GroupBy'");
        ContractAssert.FailsMentioning(result, "'Join'");
    }

    [Fact]
    public void A_query_over_a_maybe_does_not_mix_with_sequences()
    {
        var result = Snippet.Compile(Consumer.File("""
                public static object Mixed(Maybe<int> maybe) =>
                    from x in maybe
                    from y in new[] { 1, 2, 3 }
                    select x + y;
            """));

        ContractAssert.FailsWith(result, "CS1943");
    }
}
