namespace DotMaybe;

// ToNullable means Nullable<T> for value types and a nullable reference for reference types. The two overloads
// differ only in their constraints, so they live in two classes; overload resolution drops the one whose constraint
// does not hold, and callers just write maybe.ToNullable().

/// <summary>
/// <c>ToNullable</c> for maybes of value types.
/// </summary>
public static class MaybeValueTypeExtensions
{
    /// <summary>
    /// The value, or <see langword="null"/> when there is none: <c>Maybe&lt;int&gt;</c> becomes <c>int?</c>.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <param name="maybe">The maybe.</param>
    /// <returns>The value, or <see langword="null"/>.</returns>
    public static T? ToNullable<T>(this Maybe<T> maybe)
        where T : struct => maybe.Match<T?>(v => v, () => null);
}

/// <summary>
/// <c>ToNullable</c> for maybes of reference types.
/// </summary>
public static class MaybeReferenceTypeExtensions
{
    /// <summary>
    /// The value, or <see langword="null"/> when there is none: <c>Maybe&lt;string&gt;</c> becomes <c>string?</c>.
    /// </summary>
    /// <typeparam name="T">The reference type.</typeparam>
    /// <param name="maybe">The maybe.</param>
    /// <returns>The value, or <see langword="null"/>.</returns>
    public static T? ToNullable<T>(this Maybe<T> maybe)
        where T : class => maybe.Match<T?>(v => v, () => null);
}
