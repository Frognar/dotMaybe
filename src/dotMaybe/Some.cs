namespace DotMaybe;

/// <summary>
/// Provides a convenient method for creating Maybe instances with a value.
/// </summary>
public static class Some
{
    /// <summary>
    /// Creates a Maybe instance containing the specified value.
    /// </summary>
    /// <typeparam name="T">The type of the value to be wrapped in a Maybe.</typeparam>
    /// <param name="value">The value to be wrapped in a Maybe instance.</param>
    /// <returns>A Maybe instance containing the specified value.</returns>
    public static Maybe<T> With<T>(T value) => Maybe<T>.Some(value);
}
