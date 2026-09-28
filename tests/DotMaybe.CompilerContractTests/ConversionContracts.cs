namespace DotMaybe.CompilerContractTests;

/// <summary>
/// How values get into a <c>Maybe&lt;T&gt;</c>: union conversions from the value case and from <c>none</c>.
/// </summary>
public sealed class ConversionContracts
{
    [Fact]
    public void None_converts_to_a_maybe_of_any_type()
    {
        var result = Snippet.Compile(Consumer.File("""
                public static Maybe<int> Number()
                {
                    Maybe<int> maybe = none;
                    return maybe;
                }

                public static Maybe<string> Text() => none;
            """));

        ContractAssert.CompilesCleanly(result);
    }

    [Fact]
    public void A_value_converts_to_a_maybe_of_its_type()
    {
        var result = Snippet.Compile(Consumer.File("""
                public static Maybe<string> Greeting()
                {
                    Maybe<string> otherMaybe = "Hello, Monad!";
                    return otherMaybe;
                }

                public static Maybe<int> Answer() => 42;
            """));

        ContractAssert.CompilesCleanly(result);
    }

    [Fact]
    public void A_conditional_between_a_value_and_none_is_target_typed()
    {
        var result = Snippet.Compile(Consumer.File("""
                public static Maybe<int> Parse(bool ok) => ok ? 42 : none;
            """));

        ContractAssert.CompilesCleanly(result);
    }

    [Fact]
    public void Arguments_convert_to_a_maybe_parameter()
    {
        var result = Snippet.Compile(Consumer.File("""
                private static int Take(Maybe<int> maybe) => 0;

                public static int Call() => Take(42) + Take(none);
            """));

        ContractAssert.CompilesCleanly(result);
    }

    [Fact]
    public void A_value_of_a_type_parameter_converts_in_generic_code()
    {
        var result = Snippet.Compile(Consumer.File("""
                public static Maybe<T> Wrap<T>(T value)
                    where T : notnull => value;
            """));

        ContractAssert.CompilesCleanly(result);
    }

    [Fact]
    public void None_is_usable_inside_a_namespace_nested_in_DotMaybe()
    {
        // Inside DotMaybe.* the simple name None always binds to the type first.
        // The lower-case value keeps working there, which is why it is lower case.
        var result = Snippet.Compile(Consumer.File(
            """
                public static Maybe<int> Empty() => none;
            """,
            inNamespace: "DotMaybe.Consumer"));

        ContractAssert.CompilesCleanly(result);
    }

    [Fact]
    public void Converting_a_maybe_null_reference_is_flagged_by_nullable_analysis()
    {
        // Decision to confirm in TP-00: the value case is T, not T?, so nullable analysis steers
        // possibly-null input to an explicit API instead of an implicit conversion.
        // At run time a null that slips through still becomes None (TP-01).
        var result = Snippet.Compile(Consumer.File("""
                public static Maybe<string> From(string? text) => text;
            """));

        ContractAssert.WarnsAboutNullability(result);
    }
}
