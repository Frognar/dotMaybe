namespace DotMaybe;

/// <summary>
/// Provides a convenient method for creating empty Maybe instances.
/// </summary>
public static class None
{
    /// <summary>
    /// Creates an empty Maybe instance of the specified type.
    /// </summary>
    /// <typeparam name="T">The type parameter for the Maybe instance.</typeparam>
    /// <returns>An empty Maybe instance of type T.</returns>
    public static Maybe<T> OfType<T>() => Maybe<T>.None();
}
