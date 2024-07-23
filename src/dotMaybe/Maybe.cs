using System;

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

    /// <summary>
    /// Flattens a nested Maybe, reducing Maybe&lt;Maybe&lt;T&gt;&gt; to Maybe&lt;T&gt;.
    /// </summary>
    /// <typeparam name="T">The type of the value contained in the innermost Maybe.</typeparam>
    /// <param name="nested">The nested Maybe instance to flatten.</param>
    /// <returns>A flattened Maybe&lt;T&gt; instance.</returns>
    /// <remarks>
    /// This method uses the Bind operation to flatten the nested structure.
    /// If the outer Maybe is None, the result will be None.
    /// If the outer Maybe is Some, the result will be the inner Maybe.
    /// </remarks>
    public static Maybe<T> Flatten<T>(this Maybe<Maybe<T>> nested)
    {
        return nested.Bind(v => v);
    }

    /// <summary>
    /// Combines two Maybe instances using a mapping function.
    /// </summary>
    /// <typeparam name="T1">The type of the value in the first Maybe.</typeparam>
    /// <typeparam name="T2">The type of the value in the second Maybe.</typeparam>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="maybe1">The first Maybe instance.</param>
    /// <param name="maybe2">The second Maybe instance.</param>
    /// <param name="map">A function that combines the values from both Maybes if they exist.</param>
    /// <returns>
    /// A new Maybe instance containing the result of applying the map function to the values of both input Maybes
    /// if both contain values; otherwise, returns an empty Maybe.
    /// </returns>
    /// <remarks>
    /// This method is useful for combining two independent Maybe values into a single result.
    /// If either of the input Maybes is empty, the result will be an empty Maybe.
    /// </remarks>
    public static Maybe<TResult> Map2<T1, T2, TResult>(
        Maybe<T1> maybe1,
        Maybe<T2> maybe2,
        Func<T1, T2, TResult> map)
    {
        return
            from v1 in maybe1
            from v2 in maybe2
            select map(v1, v2);
    }

    /// <summary>
    /// Combines three Maybe instances using a mapping function.
    /// </summary>
    /// <typeparam name="T1">The type of the value in the first Maybe.</typeparam>
    /// <typeparam name="T2">The type of the value in the second Maybe.</typeparam>
    /// <typeparam name="T3">The type of the value in the third Maybe.</typeparam>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="maybe1">The first Maybe instance.</param>
    /// <param name="maybe2">The second Maybe instance.</param>
    /// <param name="maybe3">The third Maybe instance.</param>
    /// <param name="map">A function that combines the values from all three Maybes if they exist.</param>
    /// <returns>
    /// A new Maybe instance containing the result of applying the map function to the values of all three input Maybes
    /// if all contain values; otherwise, returns an empty Maybe.
    /// </returns>
    /// <remarks>
    /// This method is useful for combining three independent Maybe values into a single result.
    /// If any of the input Maybes is empty, the result will be an empty Maybe.
    /// </remarks>
    public static Maybe<TResult> Map3<T1, T2, T3, TResult>(
        Maybe<T1> maybe1,
        Maybe<T2> maybe2,
        Maybe<T3> maybe3,
        Func<T1, T2, T3, TResult> map)
    {
        return
            from v1 in maybe1
            from v2 in maybe2
            from v3 in maybe3
            select map(v1, v2, v3);
    }
}
