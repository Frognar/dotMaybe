using FsCheck;
using FsCheck.Fluent;
using static DotMaybe.Prelude;

namespace DotMaybe.PropertyTests;

/// <summary>
/// FsCheck generators for DotMaybe types, registered for the whole assembly in <c>AssemblyInfo.cs</c>.
/// </summary>
public static class MaybeArbitraries
{
    /// <summary>
    /// <c>Maybe&lt;T&gt;</c> for any <c>T</c> FsCheck can generate: <c>none</c> and values in equal proportion.
    /// FsCheck injects the arbitrary for <c>T</c>.
    /// </summary>
    /// <remarks>
    /// No shrinking yet: shrinking needs pattern matching on <c>Maybe&lt;T&gt;</c>, which arrives with TP-01.
    /// </remarks>
    public static Arbitrary<Maybe<T>> MaybeOf<T>(Arbitrary<T> values)
        where T : notnull
    {
        var maybes = Gen.OneOf(
            Gen.Constant(Empty<T>()),
            values.Generator.Select(value => Wrap(value)));

        return Arb.From(maybes);
    }

    private static Maybe<T> Empty<T>()
        where T : notnull => none;

    private static Maybe<T> Wrap<T>(T value)
        where T : notnull => value;
}
