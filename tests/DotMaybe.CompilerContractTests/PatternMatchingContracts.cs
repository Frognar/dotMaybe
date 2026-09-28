namespace DotMaybe.CompilerContractTests;

/// <summary>
/// Pattern matching sees exactly two cases, the value and <c>None</c>, and the compiler checks both are handled.
/// </summary>
public sealed class PatternMatchingContracts
{
    [Fact]
    public void A_switch_over_the_value_and_None_is_exhaustive()
    {
        var result = Snippet.Compile(Consumer.File("""
                public static string Describe(Maybe<int> maybe) => maybe switch
                {
                    int value => value.ToString(),
                    None => "none",
                };
            """));

        ContractAssert.CompilesCleanly(result);
    }

    [Fact]
    public void A_switch_over_a_reference_value_and_None_is_exhaustive_without_a_null_arm()
    {
        var result = Snippet.Compile(Consumer.File("""
                public static string Describe(Maybe<string> maybe) => maybe switch
                {
                    string value => value,
                    None => "none",
                };
            """));

        ContractAssert.CompilesCleanly(result);
    }

    [Fact]
    public void A_switch_over_the_value_and_None_is_exhaustive_in_generic_code()
    {
        var result = Snippet.Compile(Consumer.File("""
                public static string Describe<T>(Maybe<T> maybe)
                    where T : notnull => maybe switch
                {
                    T => "some",
                    None => "none",
                };
            """));

        ContractAssert.CompilesCleanly(result);
    }

    [Fact]
    public void A_switch_without_the_None_arm_is_not_exhaustive()
    {
        var result = Snippet.Compile(Consumer.File("""
                public static int Unwrap(Maybe<int> maybe) => maybe switch
                {
                    int value => value,
                };
            """));

        ContractAssert.WarnsSwitchIsNotExhaustive(result);
    }

    [Fact]
    public void A_switch_without_the_value_arm_is_not_exhaustive()
    {
        var result = Snippet.Compile(Consumer.File("""
                public static int Unwrap(Maybe<int> maybe) => maybe switch
                {
                    None => 0,
                };
            """));

        ContractAssert.WarnsSwitchIsNotExhaustive(result);
    }

    [Fact]
    public void Is_patterns_test_each_case()
    {
        var result = Snippet.Compile(Consumer.File("""
                public static int ValueOrZero(Maybe<int> maybe) => maybe is int value ? value : 0;

                public static bool IsEmpty(Maybe<int> maybe) => maybe is None;
            """));

        ContractAssert.CompilesCleanly(result);
    }

    [Fact]
    public void A_default_maybe_needs_no_null_arm()
    {
        // Spike for variant A. The compiler assumes that a default struct union holds a null Value,
        // while Maybe<T> defines default as None. If this fails with a null-related warning
        // (e.g. CS8655), that assumption leaks into flow analysis and we decide how to handle it.
        var result = Snippet.Compile(Consumer.File("""
                public static string Describe()
                {
                    Maybe<int> maybe = default;
                    return maybe switch
                    {
                        int value => value.ToString(),
                        None => "none",
                    };
                }
            """));

        ContractAssert.CompilesCleanly(result);
    }
}
