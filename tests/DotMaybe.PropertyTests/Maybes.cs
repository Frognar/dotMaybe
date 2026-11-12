using static DotMaybe.Prelude;

namespace DotMaybe.PropertyTests;

/// <summary>
/// Builds and reads maybes in generic code.
/// </summary>
/// <remarks>
/// Building goes through the union conversions, which are never ambiguous in generic code
/// (e.g. for <c>Maybe&lt;Maybe&lt;int&gt;&gt;</c>). Reading uses pattern matching, <c>maybe is T value</c>, which works
/// for a type parameter too (see the compiler contracts).
/// </remarks>
internal static class Maybes
{
    public static Maybe<T> Some<T>(T value)
        where T : notnull => value;

    public static Maybe<T> Empty<T>()
        where T : notnull => none;

    /// <summary>
    /// Describes a maybe independently of <c>ToString</c>, for assertion messages.
    /// </summary>
    public static string Describe<T>(Maybe<T> maybe)
        where T : notnull =>
        maybe is T value ? $"a value ({value})" : "None";
}
