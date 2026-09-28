using FsCheck;
using FsCheck.Fluent;

namespace DotMaybe.PropertyTests;

/// <summary>
/// FsCheck generators for DotMaybe types, registered for the whole assembly in <c>AssemblyInfo.cs</c>.
/// </summary>
public static class MaybeArbitraries
{
    /// <summary>
    /// <c>Maybe&lt;T&gt;</c> for any <c>T</c> FsCheck can generate: <c>none</c> and values in equal proportion.
    /// FsCheck injects the arbitrary for <c>T</c>, so nested maybes (<c>Maybe&lt;Maybe&lt;int&gt;&gt;</c>) work too.
    /// </summary>
    /// <remarks>
    /// A value shrinks to <c>none</c> first, then to the shrinks of the value itself. <c>none</c> does not shrink.
    /// </remarks>
    public static Arbitrary<Maybe<T>> MaybeOf<T>(Arbitrary<T> values)
        where T : notnull
    {
        var maybes = Gen.OneOf(
            Gen.Constant(Maybes.Empty<T>()),
            values.Generator.Select(value => Maybes.Some(value)));

        return Arb.From(maybes, maybe => Shrink(maybe, values));
    }

    private static IEnumerable<Maybe<T>> Shrink<T>(Maybe<T> maybe, Arbitrary<T> values)
        where T : notnull =>
        Maybes.TryGetValue(maybe, out var value)
            ? values.Shrinker(value)
                .Select(smaller => Maybes.Some(smaller))
                .Prepend(Maybes.Empty<T>())
            : Enumerable.Empty<Maybe<T>>();
}
