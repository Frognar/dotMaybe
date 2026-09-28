namespace DotMaybe.CompilerContractTests;

/// <summary>
/// How the combinators read at the call site: <c>OrElse</c> takes whatever converts to a maybe, <c>Zip</c> and
/// <c>Fold</c> infer their types from the arguments, <c>Flatten</c> exists only on nested maybes, and <c>Iter</c>
/// takes a method group.
/// </summary>
public sealed class CombinatorContracts
{
    [Fact]
    public void OrElse_takes_maybes_values_none_and_factories()
    {
        var result = Snippet.Compile(Consumer.File("""
                public static Maybe<int> FromMaybe(Maybe<int> maybe, Maybe<int> other) => maybe.OrElse(other);

                public static Maybe<int> FromValue(Maybe<int> maybe) => maybe.OrElse(5);

                public static Maybe<int> FromNone(Maybe<int> maybe) => maybe.OrElse(none);

                public static Maybe<int> FromConditional(Maybe<int> maybe, bool ok) => maybe.OrElse(ok ? 5 : none);

                public static Maybe<int> FromFactory(Maybe<int> maybe) => maybe.OrElse(() => 5);

                public static Maybe<int> FromNoneFactory(Maybe<int> maybe) => maybe.OrElse(() => none);
            """));

        ContractAssert.CompilesCleanly(result);
    }

    [Fact]
    public void Zip_and_Fold_infer_their_types()
    {
        var result = Snippet.Compile(Consumer.File("""
                public static Maybe<(int, string)> Pair(Maybe<int> a, Maybe<string> b) => a.Zip(b);

                public static Maybe<string> Joined(Maybe<int> a, Maybe<string> b) =>
                    a.Zip(b, (number, text) => $"{text}{number}");

                public static int Total(Maybe<int> maybe) => maybe.Fold(10, (sum, value) => sum + value);
            """));

        ContractAssert.CompilesCleanly(result);
    }

    [Fact]
    public void Flatten_works_on_nested_maybes_as_an_extension_and_as_a_function()
    {
        var result = Snippet.Compile(Consumer.File("""
                public static Maybe<int> AsExtension(Maybe<Maybe<int>> nested) => nested.Flatten();

                public static Maybe<string> AsFunction(Maybe<Maybe<string>> nested) => Maybe.Flatten(nested);
            """));

        ContractAssert.CompilesCleanly(result);
    }

    [Fact]
    public void Flatten_does_not_exist_on_a_maybe_that_is_not_nested()
    {
        var result = Snippet.Compile(Consumer.File("""
                public static object Flat(Maybe<int> maybe) => maybe.Flatten();
            """));

        ContractAssert.DoesNotCompile(result);
    }

    [Fact]
    public void None_assigned_to_a_nested_maybe_compiles()
    {
        // Maybe<Maybe<int>> has the cases Maybe<int> and None; `none` is expected to pick the outer None
        // (the property tests check the value through Maybes.Empty, which does not depend on this).
        var result = Snippet.Compile(Consumer.File("""
                public static Maybe<int> Flat()
                {
                    Maybe<Maybe<int>> nested = none;
                    return nested.Flatten();
                }
            """));

        ContractAssert.CompilesCleanly(result);
    }

    [Fact]
    public void Tap_keeps_the_chain_going()
    {
        var result = Snippet.Compile(Consumer.File("""
                public static Maybe<string> Logged(Maybe<int> maybe) =>
                    maybe
                        .Tap(Console.WriteLine)
                        .Map(number => number * 2)
                        .Tap(number => Console.WriteLine($"doubled: {number}"))
                        .Map(number => $"{number}");
            """));

        ContractAssert.CompilesCleanly(result);
    }

    [Fact]
    public void Iter_takes_a_method_group()
    {
        var result = Snippet.Compile(Consumer.File("""
                public static void Print(Maybe<int> maybe) => maybe.Iter(Console.WriteLine);
            """));

        ContractAssert.CompilesCleanly(result);
    }
}
