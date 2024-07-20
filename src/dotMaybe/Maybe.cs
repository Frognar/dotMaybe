using System;
using System.Threading.Tasks;

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
public readonly record struct Maybe<T>
{
    private readonly IMaybe _maybe;

    private Maybe(IMaybe maybe)
    {
        _maybe = maybe;
    }

    /// <summary>
    /// Marker interface for a maybe type.
    /// </summary>
    private interface IMaybe;

    /// <summary>
    /// Applies one of two functions based on whether the Maybe instance has a value or not.
    /// </summary>
    /// <typeparam name="TResult">The type of the result returned by both functions.</typeparam>
    /// <param name="none">The function to execute if the Maybe instance is empty.</param>
    /// <param name="some">The function to execute if the Maybe instance contains a value.</param>
    /// <returns>
    /// The result of executing either the 'none' function (if Maybe is empty) or
    /// the 'some' function (if Maybe contains a value).
    /// </returns>
    /// <remarks>
    /// This method provides a way to handle both cases (empty and non-empty) of a Maybe instance
    /// in a single expression, similar to pattern matching.
    /// </remarks>
    public TResult Match<TResult>(Func<TResult> none, Func<T, TResult> some)
    {
        return _maybe switch
        {
            SomeType s => some(s.Value),
            _ => none(),
        };
    }

    /// <summary>
    /// Asynchronously applies one of two functions based on whether the Maybe instance has a value or not.
    /// </summary>
    /// <typeparam name="TResult">The type of the result returned by both functions.</typeparam>
    /// <param name="none">The asynchronous function to execute if the Maybe instance is empty.</param>
    /// <param name="some">The asynchronous function to execute if the Maybe instance contains a value.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains the result of executing
    /// either the 'none' function (if Maybe is empty) or the 'some' function (if Maybe contains a value).
    /// </returns>
    public async Task<TResult> MatchAsync<TResult>(Func<Task<TResult>> none, Func<T, Task<TResult>> some)
    {
        return _maybe switch
        {
            SomeType s => await some(s.Value),
            _ => await none(),
        };
    }

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

    private readonly record struct SomeType(T Value) : IMaybe
    {
        public T Value { get; } = Value;
    }

    private readonly record struct NoneType : IMaybe;
}
