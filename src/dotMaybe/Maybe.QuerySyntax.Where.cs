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
    /// <remarks>
    /// This method is primarily used to enable LINQ query syntax for Maybe types.
    /// It allows filtering in LINQ comprehensions and can be used to chain conditions.
    /// </remarks>
    /// <example>
    /// var result = from x in Maybe&lt;int&gt;.Some(5)
    ///              where x > 3
    ///              select x;
    /// // result is Maybe&lt;int&gt; containing 5
    ///
    /// var emptyResult = from x in Maybe&lt;int&gt;.Some(2)
    ///                   where x > 3
    ///                   select x;
    /// // emptyResult is Maybe&lt;int&gt;.None().
    /// </example>
    public Maybe<T> Where(Predicate<T> predicate)
    {
        return Filter(predicate);
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
    /// <remarks>
    /// This method is primarily used to enable asynchronous LINQ query syntax for Maybe types.
    /// It allows filtering in asynchronous LINQ comprehensions and can be used to chain asynchronous conditions.
    /// </remarks>
    /// <example>
    /// var result = await (from x in Maybe&lt;int&gt;.Some(5)
    ///                     where await Task.FromResult(x > 3)
    ///                     select x);
    /// // result is Maybe&lt;int&gt; containing 5
    ///
    /// var emptyResult = await (from x in Maybe&lt;int&gt;.Some(2)
    ///                          where await Task.FromResult(x > 3)
    ///                          select x);
    /// // emptyResult is Maybe&lt;int&gt;.None().
    /// </example>
    public async Task<Maybe<T>> Where(Func<T, Task<bool>> predicate)
    {
        return await FilterAsync(predicate).ConfigureAwait(false);
    }
}
