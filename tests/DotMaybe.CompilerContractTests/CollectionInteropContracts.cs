namespace DotMaybe.CompilerContractTests;

/// <summary>
/// How the collection and interop helpers read at the call site. The consumer file imports only <c>System</c>,
/// so collection types are written out in full.
/// </summary>
public sealed class CollectionInteropContracts
{
    [Fact]
    public void Collection_helpers_read_like_LINQ()
    {
        var result = Snippet.Compile(Consumer.File("""
                public static System.Collections.Generic.IEnumerable<int> Numbers(
                    System.Collections.Generic.IEnumerable<Maybe<int>> maybes) => maybes.Choose();

                public static System.Collections.Generic.IEnumerable<int> Parsed(string[] texts) =>
                    texts.Choose(text => Maybe.Parse<int>(text));

                public static Maybe<System.Collections.Generic.IReadOnlyList<int>> All(string[] texts) =>
                    texts.Traverse(text => Maybe.Parse<int>(text));

                public static Maybe<System.Collections.Generic.IReadOnlyList<int>> AllOf(
                    System.Collections.Generic.List<Maybe<int>> maybes) => maybes.Sequence();

                public static Maybe<int> FirstPositive(int[] numbers) => numbers.FirstOrNone(number => number > 0);

                public static Maybe<string> OnlyName(System.Collections.Generic.List<string> names) =>
                    names.SingleOrNone();
            """));

        ContractAssert.CompilesCleanly(result);
    }

    [Fact]
    public void Parse_works_as_a_method_group()
    {
        var result = Snippet.Compile(Consumer.File("""
                public static System.Collections.Generic.IEnumerable<int> Parsed(string[] texts) =>
                    texts.Choose(Maybe.Parse<int>);
            """));

        ContractAssert.CompilesCleanly(result);
    }

    [Fact]
    public void GetValueOrNone_works_on_concrete_and_read_only_dictionaries()
    {
        var result = Snippet.Compile(Consumer.File("""
                public static Maybe<int> FromDictionary(System.Collections.Generic.Dictionary<string, int> ages) =>
                    ages.GetValueOrNone("Ada");

                public static Maybe<int> FromSorted(System.Collections.Generic.SortedDictionary<string, int> ages) =>
                    ages.GetValueOrNone("Ada");

                public static Maybe<int> FromReadOnly(System.Collections.Generic.IReadOnlyDictionary<string, int> ages) =>
                    ages.GetValueOrNone("Ada");
            """));

        ContractAssert.CompilesCleanly(result);
    }

    [Fact]
    public void Nullables_convert_both_ways_without_warnings()
    {
        var result = Snippet.Compile(Consumer.File("""
                public static int? Number(Maybe<int> maybe) => maybe.ToNullable();

                public static string? Text(Maybe<string> maybe) => maybe.ToNullable();

                public static Maybe<int> FromNumber(int? number) => Maybe.FromNullable(number);

                public static Maybe<string> FromText(string? text) => Maybe.FromNullable(text);
            """));

        ContractAssert.CompilesCleanly(result);
    }

    [Fact]
    public void ToNullable_needs_to_know_whether_the_type_is_a_value_or_a_reference()
    {
        // Nullable<T> and a nullable reference are different things, so unconstrained generic code uses Match.
        var result = Snippet.Compile(Consumer.File("""
                public static object? Unknown<T>(Maybe<T> maybe)
                    where T : notnull => maybe.ToNullable();
            """));

        ContractAssert.DoesNotCompile(result);
    }

    [Fact]
    public void Parse_needs_a_parsable_type()
    {
        var result = Snippet.Compile(Consumer.File("""
                public sealed class Plain
                {
                }

                public static Maybe<Plain> Read(string text) => Maybe.Parse<Plain>(text);
            """));

        ContractAssert.DoesNotCompile(result);
    }
}
