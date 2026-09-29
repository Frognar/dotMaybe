using static DotMaybe.Prelude;

namespace DotMaybe;

/// <content>
/// Maybes from nullable values and from text.
/// </content>
public static partial class Maybe
{
    /// <summary>
    /// A maybe from a nullable value type: <see langword="null"/> becomes <see cref="None"/>.
    /// </summary>
    /// <typeparam name="T">The value type.</typeparam>
    /// <param name="value">The nullable value.</param>
    /// <returns>The value, or <see cref="None"/>.</returns>
    public static Maybe<T> FromNullable<T>(T? value)
        where T : struct => value.HasValue ? value.Value : none;

    /// <summary>
    /// A maybe from a nullable reference: <see langword="null"/> becomes <see cref="None"/>.
    /// </summary>
    /// <remarks>
    /// Use it where nullable analysis sees a <c>T?</c>: converting <c>T?</c> implicitly also gives <see cref="None"/>
    /// for <see langword="null"/>, but with a nullability warning.
    /// </remarks>
    /// <typeparam name="T">The reference type.</typeparam>
    /// <param name="value">The nullable reference.</param>
    /// <returns>The value, or <see cref="None"/>.</returns>
    public static Maybe<T> FromNullable<T>(T? value)
        where T : class => value is not null ? value : none;

    /// <summary>
    /// Parses <paramref name="text"/> with <c>T.TryParse</c>: the parsed value, or <see cref="None"/> when the text is
    /// <see langword="null"/> or not valid.
    /// </summary>
    /// <remarks>Uses the current culture, like <c>T.TryParse</c> without a format provider.</remarks>
    /// <typeparam name="T">The type to parse.</typeparam>
    /// <param name="text">The text to parse.</param>
    /// <returns>The parsed value, or <see cref="None"/>.</returns>
    public static Maybe<T> Parse<T>(string? text)
        where T : notnull, IParsable<T> => T.TryParse(text, null, out var value) ? value : none;

    /// <summary>
    /// Parses <paramref name="text"/> with <c>T.TryParse</c> and <paramref name="provider"/>: the parsed value, or
    /// <see cref="None"/> when the text is <see langword="null"/> or not valid.
    /// </summary>
    /// <typeparam name="T">The type to parse.</typeparam>
    /// <param name="text">The text to parse.</param>
    /// <param name="provider">Culture-specific formatting information, or <see langword="null"/> for the current culture.</param>
    /// <returns>The parsed value, or <see cref="None"/>.</returns>
    public static Maybe<T> Parse<T>(string? text, IFormatProvider? provider)
        where T : notnull, IParsable<T> => T.TryParse(text, provider, out var value) ? value : none;
}
