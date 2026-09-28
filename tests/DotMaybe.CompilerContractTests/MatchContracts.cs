namespace DotMaybe.CompilerContractTests;

/// <summary>
/// The shape of <c>Match</c> and <c>OrDefault</c> as consumers see it: parameter names and order (they are part of the
/// public API once named arguments use them), nullability annotations and overload selection.
/// </summary>
public sealed class MatchContracts
{
    [Fact]
    public void Match_reads_the_value_in_generic_code()
    {
        var result = Snippet.Compile(Consumer.File("""
                public static string Describe<T>(Maybe<T> maybe)
                    where T : notnull => maybe.Match(value => $"some {value}", () => "none");
            """));

        ContractAssert.CompilesCleanly(result);
    }

    [Fact]
    public void Arguments_can_be_named_some_none_and_fallback()
    {
        var result = Snippet.Compile(Consumer.File("""
                public static int Unwrap(Maybe<int> maybe) => maybe.Match(some: value => value, none: () => 0);

                public static int Reversed(Maybe<int> maybe) => maybe.Match(none: () => 0, some: value => value);

                public static int Fallback(Maybe<int> maybe) => maybe.OrDefault(fallback: 0);

                public static int LazyFallback(Maybe<int> maybe) => maybe.OrDefault(fallback: () => 0);
            """));

        ContractAssert.CompilesCleanly(result);
    }

    [Fact]
    public void A_lambda_fallback_selects_the_factory_overload()
    {
        var result = Snippet.Compile(Consumer.File("""
                public static string Text(Maybe<string> maybe) => maybe.OrDefault(() => "fallback");

                public static int Number(Maybe<int> maybe) => maybe.OrDefault(() => 42);
            """));

        ContractAssert.CompilesCleanly(result);
    }

    [Fact]
    public void Match_branches_are_not_nullable()
    {
        var result = Snippet.Compile(Consumer.File("""
                public static int Broken(Maybe<int> maybe) => maybe.Match(null, () => 0);
            """));

        ContractAssert.WarnsAboutNullability(result);
    }
}
