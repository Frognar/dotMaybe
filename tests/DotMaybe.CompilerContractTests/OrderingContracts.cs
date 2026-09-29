namespace DotMaybe.CompilerContractTests;

/// <summary>
/// Maybes sort with the standard APIs and satisfy <c>IComparable&lt;T&gt;</c> constraints, but relational operators
/// are deliberately missing: <c>age &gt;= 18</c> must not compile and silently order <c>none</c> below 18.
/// </summary>
public sealed class OrderingContracts
{
    [Fact]
    public void Maybes_sort_with_the_standard_APIs()
    {
        var result = Snippet.Compile(Consumer.File("""
                public static System.Collections.Generic.IEnumerable<Maybe<int>> Sorted(Maybe<int>[] maybes) =>
                    System.Linq.Enumerable.Order(maybes);

                public static void SortInPlace(System.Collections.Generic.List<Maybe<string>> maybes) => maybes.Sort();

                public static Maybe<int> Latest(Maybe<int>[] maybes) => System.Linq.Enumerable.Max(maybes);

                public static int Compare(Maybe<int> a, Maybe<int> b) =>
                    System.Collections.Generic.Comparer<Maybe<int>>.Default.Compare(a, b);
            """));

        ContractAssert.CompilesCleanly(result);
    }

    [Fact]
    public void Maybes_satisfy_comparable_constraints()
    {
        var result = Snippet.Compile(Consumer.File("""
                private static T Larger<T>(T a, T b)
                    where T : IComparable<T> => a.CompareTo(b) >= 0 ? a : b;

                public static Maybe<int> Pick(Maybe<int> a, Maybe<int> b) => Larger(a, b);
            """));

        ContractAssert.CompilesCleanly(result);
    }

    [Fact]
    public void Relational_operators_are_not_defined()
    {
        var result = Snippet.Compile(Consumer.File("""
                public static bool Adult(Maybe<int> age) => age >= 18;

                public static bool Before(Maybe<int> a, Maybe<int> b) => a < b;
            """));

        ContractAssert.FailsMentioning(result, "'>='");
        ContractAssert.FailsMentioning(result, "'<'");
    }
}
