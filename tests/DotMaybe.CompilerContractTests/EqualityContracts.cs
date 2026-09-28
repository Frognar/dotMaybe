namespace DotMaybe.CompilerContractTests;

/// <summary>
/// The equality operators read naturally with both cases on either side: <c>maybe == none</c>, <c>maybe == 42</c>.
/// </summary>
/// <remarks>
/// The operators are declared for <c>(Maybe&lt;T&gt;, Maybe&lt;T&gt;)</c> only; these contracts check that the union
/// conversions let <c>none</c> and plain values take part in overload resolution. If they fail, we add
/// dedicated overloads instead.
/// </remarks>
public sealed class EqualityContracts
{
    [Fact]
    public void A_maybe_compares_with_none()
    {
        var result = Snippet.Compile(Consumer.File("""
                public static bool IsEmpty(Maybe<int> maybe) => maybe == none;

                public static bool HasValue(Maybe<int> maybe) => maybe != none;

                public static bool IsEmptyReversed(Maybe<int> maybe) => none == maybe;
            """));

        ContractAssert.CompilesCleanly(result);
    }

    [Fact]
    public void A_maybe_compares_with_a_value()
    {
        var result = Snippet.Compile(Consumer.File("""
                public static bool IsAnswer(Maybe<int> maybe) => maybe == 42;

                public static bool IsNotAnswer(Maybe<int> maybe) => 42 != maybe;

                public static bool IsGreeting(Maybe<string> maybe) => maybe == "Hello, Monad!";
            """));

        ContractAssert.CompilesCleanly(result);
    }

    [Fact]
    public void Two_maybes_compare_with_the_operators()
    {
        var result = Snippet.Compile(Consumer.File("""
                public static bool Same(Maybe<int> left, Maybe<int> right) => left == right;

                public static bool Different(Maybe<int> left, Maybe<int> right) => left != right;
            """));

        ContractAssert.CompilesCleanly(result);
    }

    [Fact]
    public void Maybes_of_different_types_do_not_compare()
    {
        var result = Snippet.Compile(Consumer.File("""
                public static bool Mixed(Maybe<int> number, Maybe<string> text) => number == text;
            """));

        ContractAssert.FailsWith(result, "CS0019");
    }
}
