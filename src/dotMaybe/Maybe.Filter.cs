using System;
using System.Threading.Tasks;

namespace DotMaybe;

public readonly partial record struct Maybe<T>
{
    /// <summary>
    /// Filters the value in the Maybe based on a predicate.
    /// </summary>
    /// <param name="predicate">A function to test the value against a condition.</param>
    /// <returns>
    /// The original Maybe if it contains a value that satisfies the predicate;
    /// otherwise, returns an empty Maybe.
    /// </returns>
    public Maybe<T> Filter(Predicate<T> predicate)
    {
        return _maybe switch
        {
            NoneType => this,
            SomeType some when predicate(some.Value) => this,
            _ => None(),
        };
    }

    /// <summary>
    /// Asynchronously filters the value in the Maybe based on a predicate.
    /// </summary>
    /// <param name="predicate">An asynchronous function to test the value against a condition.</param>
    /// <returns>
    /// A Task that represents the asynchronous operation.
    /// The task result contains the original Maybe if it contains a value that satisfies the predicate;
    /// An empty Maybe otherwise.
    /// </returns>
    public async Task<Maybe<T>> FilterAsync(Func<T, Task<bool>> predicate)
    {
        return _maybe switch
        {
            NoneType => this,
            SomeType some when await predicate(some.Value).ConfigureAwait(false) => this,
            _ => None(),
        };
    }
}
