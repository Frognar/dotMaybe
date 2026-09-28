using System.Diagnostics.CodeAnalysis;
using static DotMaybe.Prelude;

namespace DotMaybe.PropertyTests;

/// <summary>
/// Builds and reads maybes in generic code.
/// </summary>
/// <remarks>
/// Building goes through the union conversions, which are never ambiguous in generic code
/// (e.g. for <c>Maybe&lt;Maybe&lt;int&gt;&gt;</c>). Reading goes through the union members, because C# 15 rejects
/// <c>maybe is T value</c> when <c>T</c> is a type parameter (CS8780, see the compiler contracts).
/// Tests on concrete types read values with pattern matching instead.
/// </remarks>
internal static class Maybes
{
    public static Maybe<T> Some<T>(T value)
        where T : notnull => value;

    public static Maybe<T> Empty<T>()
        where T : notnull => none;

    public static bool TryGetValue<T>(Maybe<T> maybe, [MaybeNullWhen(false)] out T value)
        where T : notnull
    {
        Maybe<T>.IUnionMembers members = maybe;
        return members.TryGetValue(out value);
    }

    /// <summary>
    /// Describes a maybe independently of <c>ToString</c>, for assertion messages.
    /// </summary>
    public static string Describe<T>(Maybe<T> maybe)
        where T : notnull =>
        TryGetValue(maybe, out var value) ? $"a value ({value})" : "None";
}
