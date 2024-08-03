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

/// <summary>
/// Provides extension methods for working with Maybe types in asynchronous contexts.
/// </summary>
public static partial class MaybeExtensions
{
    /// <summary>
    /// Asynchronously filters the value in a Task&lt;Maybe&lt;T&gt;&gt; based on a synchronous predicate.
    /// </summary>
    /// <typeparam name="T">The type of the value in the Maybe.</typeparam>
    /// <param name="source">The Task&lt;Maybe&lt;T&gt;&gt; to filter.</param>
    /// <param name="predicate">A function to test the value against a condition.</param>
    /// <returns>
    /// A Task that represents the asynchronous operation.
    /// The task result contains:
    /// The original Maybe if it contains a value that satisfies the predicate;
    /// otherwise, returns an empty Maybe.
    /// </returns>
    /// <remarks>
    /// This method allows for filtering a Task&lt;Maybe&lt;T&gt;&gt; using a synchronous predicate,
    /// enabling easier integration with asynchronous workflows.
    /// </remarks>
    public static async Task<Maybe<T>> Where<T>(this Task<Maybe<T>> source, Predicate<T> predicate)
    {
        return (await source.ConfigureAwait(false)).Where(predicate);
    }

    /// <summary>
    /// Asynchronously filters the value in a Task&lt;Maybe&lt;T&gt;&gt; based on an asynchronous predicate.
    /// </summary>
    /// <typeparam name="T">The type of the value in the Maybe.</typeparam>
    /// <param name="source">The Task&lt;Maybe&lt;T&gt;&gt; to filter.</param>
    /// <param name="predicate">An asynchronous function to test the value against a condition.</param>
    /// <returns>
    /// A Task that represents the asynchronous operation.
    /// The task result contains:
    /// The original Maybe if it contains a value that satisfies the predicate;
    /// otherwise, returns an empty Maybe.
    /// </returns>
    /// <remarks>
    /// This method allows for filtering a Task&lt;Maybe&lt;T&gt;&gt; using an asynchronous predicate,
    /// providing full support for asynchronous operations in the filtering process.
    /// </remarks>
    public static async Task<Maybe<T>> Where<T>(this Task<Maybe<T>> source, Func<T, Task<bool>> predicate)
    {
        return await (await source.ConfigureAwait(false))
            .Where(async v => await predicate(v).ConfigureAwait(false))
            .ConfigureAwait(false);
    }
}
