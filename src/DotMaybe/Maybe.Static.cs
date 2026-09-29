namespace DotMaybe;

/// <summary>
/// Functions on maybes that cannot be members of <see cref="Maybe{T}"/>, because they need a more specific shape
/// of maybe (for example a maybe of a maybe) or create maybes from other types.
/// </summary>
public static partial class Maybe
{
    /// <summary>
    /// Removes one level of nesting: <c>Some(Some(x))</c> becomes <c>Some(x)</c>; <c>Some(None)</c> and
    /// <see cref="None"/> become <see cref="None"/>.
    /// </summary>
    /// <remarks>
    /// <c>maybe.Map(f).Flatten()</c> is <c>maybe.Bind(f)</c>.
    /// </remarks>
    /// <typeparam name="T">The type of the inner value.</typeparam>
    /// <param name="nested">The maybe of a maybe.</param>
    /// <returns>The inner maybe, or <see cref="None"/>.</returns>
    public static Maybe<T> Flatten<T>(this Maybe<Maybe<T>> nested)
        where T : notnull => nested.Bind(maybe => maybe);
}
