using System;
using System.Threading.Tasks;

namespace DotMaybe;

public readonly partial record struct Maybe<T>
{
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
    /// Applies a synchronous function for the empty case and an asynchronous function for the non-empty case.
    /// </summary>
    /// <typeparam name="TResult">The type of the result returned by both functions.</typeparam>
    /// <param name="none">The synchronous function to execute if the Maybe instance is empty.</param>
    /// <param name="some">The asynchronous function to execute if the Maybe instance contains a value.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains the result of executing
    /// either the 'none' function (if Maybe is empty) or the 'some' function (if Maybe contains a value).
    /// </returns>
    public async Task<TResult> MatchAsync<TResult>(Func<TResult> none, Func<T, Task<TResult>> some)
    {
        return _maybe switch
        {
            SomeType s => await some(s.Value),
            _ => none(),
        };
    }
}
