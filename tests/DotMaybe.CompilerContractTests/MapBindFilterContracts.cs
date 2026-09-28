namespace DotMaybe.CompilerContractTests;

/// <summary>
/// How <c>Map</c>, <c>Bind</c> and <c>Filter</c> read at the call site: chaining, type inference and the
/// <c>notnull</c> constraint on the result type.
/// </summary>
public sealed class MapBindFilterContracts
{
    [Fact]
    public void Steps_chain_without_type_arguments()
    {
        var result = Snippet.Compile(Consumer.File("""
                private static Maybe<int> Parse(string text) => int.TryParse(text, out var number) ? number : none;

                public static Maybe<string> Pipeline(Maybe<string> input) =>
                    input
                        .Filter(text => text.Length > 0)
                        .Bind(Parse)
                        .Map(number => number * 2)
                        .Map(number => number.ToString());
            """));

        ContractAssert.CompilesCleanly(result);
    }

    [Fact]
    public void Bind_with_an_explicit_result_type_accepts_a_bare_value_or_none()
    {
        var result = Snippet.Compile(Consumer.File("""
                public static Maybe<int> Positive(Maybe<int> maybe) =>
                    maybe.Bind<int>(number => number > 0 ? number : none);
            """));

        ContractAssert.CompilesCleanly(result);
    }

    [Fact]
    public void Bind_cannot_infer_the_result_type_from_a_bare_value_or_none()
    {
        // The lambda's `number > 0 ? number : none` has no natural type, so type inference has nothing to go on
        // (CS0411) and callers write Bind<int>(...) as above. If a later compiler infers Maybe<int> here, this
        // contract turns red: good news, and the docs can drop the explicit type argument.
        var result = Snippet.Compile(Consumer.File("""
                public static Maybe<int> Positive(Maybe<int> maybe) =>
                    maybe.Bind(number => number > 0 ? number : none);
            """));

        ContractAssert.FailsWith(result, "CS0411");
    }

    [Fact]
    public void Map_to_a_nullable_type_is_flagged()
    {
        // TResult is notnull: a nullable result type is a warning, and at run time a null result becomes None.
        var result = Snippet.Compile(Consumer.File("""
                public static Maybe<string?> Broken(Maybe<int> maybe) => maybe.Map(number => (string?)null);
            """));

        ContractAssert.WarnsWith(result, "CS8714");
    }
}
