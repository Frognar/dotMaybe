namespace DotMaybe.CompilerContractTests;

/// <summary>
/// The union plumbing stays out of the public API: the only ways in are the conversions,
/// and the only ways out are pattern matching and (from TP-02 on) total functions.
/// </summary>
public sealed class EncapsulationContracts
{
    [Fact]
    public void Value_is_not_part_of_the_public_api()
    {
        var result = Snippet.Compile(Consumer.File("""
                                                       public static object Peek(Maybe<int> maybe) => maybe.Value;
                                                   """));

        ContractAssert.FailsWith(result, "CS1061");
    }

    [Fact]
    public void TryGetValue_is_not_part_of_the_public_api()
    {
        var result = Snippet.Compile(Consumer.File("""
                                                       public static bool Peek(Maybe<int> maybe) => maybe.TryGetValue(out int value);
                                                   """));

        ContractAssert.FailsWith(result, "CS1061");
    }

    [Fact]
    public void No_public_constructor_takes_a_value()
    {
        var result = Snippet.Compile(Consumer.File("""
                                                       public static Maybe<int> Make() => new Maybe<int>(42);
                                                   """));

        ContractAssert.FailsWith(result, "CS1729");
    }
}
