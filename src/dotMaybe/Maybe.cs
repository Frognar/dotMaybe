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
    public static Maybe<T> Some(T value) => new(new SomeType(value));

    /// <summary>
    /// Creates an empty Maybe instance of the specified type.
    /// </summary>
    /// <returns>An empty Maybe instance of type T.</returns>
    public static Maybe<T> None() => new(default(NoneType));
}
