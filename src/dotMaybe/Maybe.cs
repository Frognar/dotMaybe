namespace DotMaybe;

/// <summary>
/// Represents an optional value of type T.
/// </summary>
/// <typeparam name="T">The type of the value that may or may not be present.</typeparam>
/// /// <remarks>
/// The Maybe monad is used to handle potentially missing values without using null references.
/// It encapsulates the concept of an optional value, providing a safe way to represent and
/// manipulate data that may or may not exist.
/// </remarks>
public readonly partial record struct Maybe<T>
{
    private readonly IMaybe _maybe;

    private Maybe(IMaybe maybe)
    {
        _maybe = maybe;
    }

    /// <summary>
    /// Gets a value indicating whether this Maybe instance contains a value.
    /// </summary>
    public bool IsSome => _maybe is SomeType;

    /// <summary>
    /// Gets a value indicating whether this Maybe instance is empty.
    /// </summary>
    public bool IsNone => _maybe is NoneType;

    /// <summary>
    /// Creates a Maybe instance containing the specified value.
    /// </summary>
    /// <param name="value">The value to be wrapped in a Maybe instance.</param>
    /// <returns>A Maybe instance containing the specified value.</returns>
    public static implicit operator Maybe<T>(T? value) => value is not null ? Some(value) : None();

    /// <summary>
    /// Creates a Maybe instance containing the specified value.
    /// </summary>
    /// <param name="value">The value to be wrapped in a Maybe instance.</param>
    /// <returns>A Maybe instance containing the specified value.</returns>
    public static Maybe<T> Some(T value) => new(new SomeType(value));

    /// <summary>
    /// Creates an empty Maybe instance of the specified type.
    /// </summary>
    /// <returns>An empty Maybe instance of type T.</returns>
    public static Maybe<T> None() => new(default(NoneType));
}

/// <summary>
/// Provides static methods for working with Maybe types.
/// </summary>
public static class Maybe
{
    /// <summary>
    /// Converts a value to a Maybe instance.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="value">The value to convert to a Maybe instance.</param>
    /// <returns>
    /// A Maybe instance containing the value if it's not null; otherwise, returns an empty Maybe.
    /// </returns>
    /// <remarks>
    /// This method provides a convenient way to create a Maybe instance from any value,
    /// handling both reference types and value types.
    /// For reference types, it returns None if the value is null.
    /// For value types, it always returns a Some instance, as value types cannot be null.
    /// </remarks>
    public static Maybe<T> ToMaybe<T>(this T value)
    {
        return value is null ? Maybe<T>.None() : Maybe<T>.Some(value);
    }

    /// <summary>
    /// Converts a nullable value type to a Maybe instance.
    /// </summary>
    /// <typeparam name="T">The type of the value, which must be a value type.</typeparam>
    /// <param name="value">The nullable value to convert.</param>
    /// <returns>
    /// A Maybe instance containing the value if it has a value; otherwise, returns an empty Maybe.
    /// </returns>
    public static Maybe<T> ToMaybe<T>(this T? value)
        where T : struct
    {
        return value.HasValue ? Maybe<T>.Some(value.Value) : Maybe<T>.None();
    }
}
