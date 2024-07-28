using System;
using System.Collections.Generic;
using System.Linq;

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
/// Provides extension methods for working with Maybe types.
/// </summary>
public static partial class MaybeExtensions
{
    /// <summary>
    /// Returns the first element of a sequence as a Maybe, or an empty Maybe if the sequence contains no elements.
    /// </summary>
    /// <typeparam name="T">The type of elements in the sequence.</typeparam>
    /// <param name="source">The IEnumerable&lt;T&gt; to return the first element from.</param>
    /// <returns>
    /// A Maybe&lt;T&gt; containing the first element of the sequence if it exists;
    /// otherwise, returns an empty Maybe.
    /// </returns>
    /// <remarks>
    /// This method is similar to FirstOrDefault(), but wraps the result in a Maybe type.
    /// It handles both null and non-null elements in the sequence.
    /// </remarks>
    public static Maybe<T> FirstOrNone<T>(this IEnumerable<T> source)
    {
        return source.Select(Some.With).DefaultIfEmpty(None.OfType<T>()).First();
    }

    /// <summary>
    /// Returns the first element of a sequence that satisfies a specified condition as a Maybe,
    /// or an empty Maybe if no such element is found.
    /// </summary>
    /// <typeparam name="T">The type of elements in the sequence.</typeparam>
    /// <param name="source">The IEnumerable&lt;T&gt; to return the first element from.</param>
    /// <param name="predicate">A function to test each element for a condition.</param>
    /// <returns>
    /// A Maybe&lt;T&gt; containing the first element in the sequence that satisfies the condition if it exists;
    /// otherwise, returns an empty Maybe.
    /// </returns>
    /// <remarks>
    /// This method is similar to FirstOrDefault(predicate), but wraps the result in a Maybe type.
    /// It handles both null and non-null elements in the sequence.
    /// </remarks>
    public static Maybe<T> FirstOrNone<T>(this IEnumerable<T> source, Func<T, bool> predicate)
    {
        return source.Where(predicate).Select(Some.With).DefaultIfEmpty(None.OfType<T>()).First();
    }

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
}
