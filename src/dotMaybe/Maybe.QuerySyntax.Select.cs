using System;
using System.Threading.Tasks;

namespace DotMaybe;

public readonly partial record struct Maybe<T>
{
    /// <summary>
    /// Projects the value of a Maybe into a new form if it exists.
    /// </summary>
    /// <typeparam name="TResult">The type of the value in the resulting Maybe.</typeparam>
    /// <param name="selector">A transform function to apply to the value if it exists.</param>
    /// <returns>
    /// A new Maybe instance containing the transformed value if the original Maybe had a value;
    /// otherwise, returns an empty Maybe of the new type.
    /// </returns>
    /// <remarks>
    /// This method is primarily used to enable LINQ query syntax for Maybe types.
    /// It is equivalent to the Map method and allows Maybe to work with LINQ comprehension syntax.
    /// </remarks>
    /// <example>
    /// var result = from x in Maybe&lt;int&gt;.Some(5)
    ///              select x * 2;
    /// // result is Maybe&lt;int&gt; containing 10.
    /// </example>
    public Maybe<TResult> Select<TResult>(Func<T, TResult> selector)
    {
        return Map(selector);
    }

    /// <summary>
    /// Asynchronously projects the value of a Maybe into a new form if it exists.
    /// </summary>
    /// <typeparam name="TResult">The type of the value in the resulting Maybe.</typeparam>
    /// <param name="selector">An asynchronous transform function to apply to the value if it exists.</param>
    /// <returns>
    /// A Task that represents the asynchronous operation. The task result is a new Maybe instance
    /// containing the transformed value if the original Maybe had a value;
    /// otherwise, it's an empty Maybe of the new type.
    /// </returns>
    /// <remarks>
    /// This method enables LINQ query syntax for Maybe types in asynchronous contexts.
    /// It is equivalent to the MapAsync method and allows Maybe to work with asynchronous LINQ comprehension syntax.
    /// </remarks>
    /// <example>
    /// var result = await (from x in Maybe&lt;int&gt;.Some(5)
    ///                     select FetchDataAsync(x));
    /// // result is Maybe&lt;Data&gt; containing the fetched data if successful.
    /// </example>
    public async Task<Maybe<TResult>> Select<TResult>(Func<T, Task<TResult>> selector)
    {
        return await MapAsync(selector).ConfigureAwait(false);
    }
}

/// <summary>
/// Provides extension methods for working with Maybe types in asynchronous contexts.
/// </summary>
public static partial class MaybeExtensions
{
    /// <summary>
    /// Asynchronously projects the value of a Maybe wrapped in a Task into a new form if it exists.
    /// </summary>
    /// <typeparam name="T">The type of the value in the source Maybe.</typeparam>
    /// <typeparam name="TResult">The type of the value in the resulting Maybe.</typeparam>
    /// <param name="source">A Task containing a Maybe to transform.</param>
    /// <param name="selector">A transform function to apply to the value if it exists.</param>
    /// <returns>
    /// A Task that represents the asynchronous operation. The task result is a new Maybe instance
    /// containing the transformed value if the original Maybe had a value;
    /// otherwise, it's an empty Maybe of the new type.
    /// </returns>
    /// <remarks>
    /// This method enables LINQ query syntax for Maybe types wrapped in Tasks.
    /// It is equivalent to the MapAsync method and allows for chaining transformations in asynchronous workflows.
    /// </remarks>
    /// <example>
    /// Task&lt;Maybe&lt;int&gt;&gt; maybeIntTask = FetchMaybeIntAsync();
    /// var result = await (from x in maybeIntTask
    ///                     select x * 2);
    /// // result is Maybe&lt;int&gt; containing the doubled value if original Maybe had a value.
    /// </example>
    public static async Task<Maybe<TResult>> Select<T, TResult>(this Task<Maybe<T>> source, Func<T, TResult> selector)
    {
        return await source.MapAsync(selector).ConfigureAwait(false);
    }

    /// <summary>
    /// Asynchronously projects the value of a Maybe wrapped in a Task into a new form if it exists, using an asynchronous selector.
    /// </summary>
    /// <typeparam name="T">The type of the value in the source Maybe.</typeparam>
    /// <typeparam name="TResult">The type of the value in the resulting Maybe.</typeparam>
    /// <param name="source">A Task containing a Maybe to transform.</param>
    /// <param name="selector">An asynchronous transform function to apply to the value if it exists.</param>
    /// <returns>
    /// A Task that represents the asynchronous operation. The task result is a new Maybe instance
    /// containing the transformed value if the original Maybe had a value;
    /// otherwise, it's an empty Maybe of the new type.
    /// </returns>
    /// <remarks>
    /// This method enables LINQ query syntax for Maybe types wrapped in Tasks, with asynchronous selectors.
    /// It is equivalent to the MapAsync method with an asynchronous selector and allows for chaining
    /// asynchronous transformations in asynchronous workflows.
    /// </remarks>
    /// <example>
    /// Task&lt;Maybe&lt;int&gt;&gt; maybeIntTask = FetchMaybeIntAsync();
    /// var result = await (from x in maybeIntTask
    ///                     select FetchDataAsync(x));
    /// // result is Maybe&lt;Data&gt; containing the fetched data if original Maybe had a value and fetch was successful.
    /// </example>
    public static async Task<Maybe<TResult>> Select<T, TResult>(this Task<Maybe<T>> source, Func<T, Task<TResult>> selector)
    {
        return await source.MapAsync(selector).ConfigureAwait(false);
    }
}
