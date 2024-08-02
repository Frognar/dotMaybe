using System;
using System.Threading.Tasks;

namespace DotMaybe;

public readonly partial record struct Maybe<T>
{
    /// <summary>
    /// Projects the value of a Maybe into a new Maybe, then flattens the result into a single Maybe.
    /// </summary>
    /// <typeparam name="TIntermediate">The type of the intermediate Maybe.</typeparam>
    /// <typeparam name="TResult">The type of the value in the resulting Maybe.</typeparam>
    /// <param name="intermediateSelector">A transform function to apply to the source value, producing an intermediate Maybe.</param>
    /// <param name="resultSelector">A transform function to apply to the source value and the intermediate value.</param>
    /// <returns>
    /// A new Maybe instance containing the result of the transforms if all steps produce values;
    /// otherwise, returns an empty Maybe.
    /// </returns>
    /// <remarks>
    /// This method is primarily used to enable LINQ query syntax for Maybe types, specifically for supporting
    /// multiple 'from' clauses and 'let' clauses in LINQ comprehensions.
    /// It allows for flattening of nested Maybes and chaining of Maybe-producing operations.
    /// </remarks>
    /// <example>
    /// var result = from x in Maybe&lt;int&gt;.Some(5)
    ///              from y in Maybe&lt;int&gt;.Some(x * 2)
    ///              select x + y;
    /// // result is Maybe&lt;int&gt; containing 15.
    /// </example>
    public Maybe<TResult> SelectMany<TIntermediate, TResult>(
        Func<T, Maybe<TIntermediate>> intermediateSelector,
        Func<T, TIntermediate, TResult> resultSelector)
    {
        return Bind(v1 => intermediateSelector(v1).Map(v2 => resultSelector(v1, v2)));
    }

    /// <summary>
    /// Projects and flattens a Maybe value using a synchronous intermediate selector and an asynchronous result selector.
    /// </summary>
    /// <typeparam name="TIntermediate">The type of the intermediate Maybe.</typeparam>
    /// <typeparam name="TResult">The type of the value in the resulting Maybe.</typeparam>
    /// <param name="intermediateSelector">A transform function to apply to the source value, producing an intermediate Maybe.</param>
    /// <param name="resultSelector">An asynchronous transform function to apply to the source value and the intermediate value.</param>
    /// <returns>
    /// A Task representing the asynchronous operation, containing a new Maybe instance containing the result of the transforms if all steps produce values;
    /// otherwise, returns an empty Maybe.
    /// </returns>
    /// <remarks>
    /// This method is primarily used to enable complex LINQ query syntax for Maybe types in asynchronous context, specifically for supporting
    /// multiple 'from' clauses and 'let' clauses in LINQ comprehensions.
    /// It allows for flattening of nested Maybes and chaining of Maybe-producing operations.
    /// </remarks>
    /// <example>
    /// var result = await (from x in maybeInt
    ///                     from y in GetMaybeString(x)
    ///                     select FetchDataAsync(x, y));
    /// // Where GetMaybeString returns Maybe&lt;string&gt; and FetchDataAsync is asynchronous.
    /// </example>
    public async Task<Maybe<TResult>> SelectMany<TIntermediate, TResult>(
        Func<T, Maybe<TIntermediate>> intermediateSelector,
        Func<T, TIntermediate, Task<TResult>> resultSelector)
    {
        return await BindAsync(async v1 => await intermediateSelector(v1)
            .MapAsync(async v2 => await resultSelector(v1, v2).ConfigureAwait(false))
            .ConfigureAwait(false)).ConfigureAwait(false);
    }

    /// <summary>
    /// Projects and flattens a Maybe value using an asynchronous intermediate selector and a synchronous result selector.
    /// </summary>
    /// <typeparam name="TIntermediate">The type of the intermediate Maybe.</typeparam>
    /// <typeparam name="TResult">The type of the final result.</typeparam>
    /// <param name="intermediateSelector">An asynchronous function that returns a Maybe of an intermediate value.</param>
    /// <param name="resultSelector">A function that combines the source value and intermediate value into a result.</param>
    /// <returns>A Task representing the asynchronous operation, containing a Maybe of the final result.</returns>
    /// <remarks>
    /// This method enables complex LINQ query syntax for Maybe types in asynchronous contexts.
    /// It allows for chaining of Maybe-producing operations and flattening of nested Maybes.
    /// The method uses ConfigureAwait(false) to avoid forcing continuations back to the original context.
    /// </remarks>
    /// <example>
    /// var result = await (from x in maybeInt
    ///                     from y in GetMaybeStringAsync(x)
    ///                     select CombineValues(x, y));
    /// // Where GetMaybeStringAsync returns Task&lt;Maybe&lt;string&gt;&gt; and CombineValues is synchronous.
    /// </example>
    public async Task<Maybe<TResult>> SelectMany<TIntermediate, TResult>(
        Func<T, Task<Maybe<TIntermediate>>> intermediateSelector,
        Func<T, TIntermediate, TResult> resultSelector)
    {
        return await BindAsync(async v1 => (await intermediateSelector(v1).ConfigureAwait(false))
            .Map(v2 => resultSelector(v1, v2))).ConfigureAwait(false);
    }

    /// <summary>
    /// Projects and flattens a Maybe value using both asynchronous intermediate and result selectors.
    /// </summary>
    /// <typeparam name="TIntermediate">The type of the intermediate Maybe.</typeparam>
    /// <typeparam name="TResult">The type of the final result.</typeparam>
    /// <param name="intermediateSelector">An asynchronous function that returns a Maybe of an intermediate value.</param>
    /// <param name="resultSelector">An asynchronous function that combines the source value and intermediate value into a result.</param>
    /// <returns>A Task representing the asynchronous operation, containing a Maybe of the final result.</returns>
    /// <remarks>
    /// This method enables complex LINQ query syntax for Maybe types in fully asynchronous contexts.
    /// It allows for chaining of asynchronous Maybe-producing operations and flattening of nested Maybes.
    /// The method uses ConfigureAwait(false) extensively to avoid forcing continuations back to the original context,
    /// which is particularly important for performance in deeply nested asynchronous operations.
    /// </remarks>
    /// <example>
    /// var result = await (from x in maybeInt
    ///                     from y in GetMaybeStringAsync(x)
    ///                     select FetchDataAsync(x, y));
    /// // Where both GetMaybeStringAsync and FetchDataAsync are asynchronous operations.
    /// </example>
    public async Task<Maybe<TResult>> SelectMany<TIntermediate, TResult>(
        Func<T, Task<Maybe<TIntermediate>>> intermediateSelector,
        Func<T, TIntermediate, Task<TResult>> resultSelector)
    {
        return await BindAsync(async v1 => await (await intermediateSelector(v1).ConfigureAwait(false))
            .MapAsync(async v2 => await resultSelector(v1, v2).ConfigureAwait(false))
            .ConfigureAwait(false)).ConfigureAwait(false);
    }
}

/// <summary>
/// Provides extension methods for working with Maybe types in asynchronous contexts.
/// </summary>
public static partial class MaybeExtensions
{
    /// <summary>
    /// Projects and flattens a Maybe value wrapped in a Task, using synchronous selectors.
    /// </summary>
    /// <typeparam name="T">The type of the source Maybe value.</typeparam>
    /// <typeparam name="TIntermediate">The type of the intermediate Maybe.</typeparam>
    /// <typeparam name="TResult">The type of the final result.</typeparam>
    /// <param name="source">A Task containing the source Maybe to transform.</param>
    /// <param name="intermediateSelector">A function that returns a Maybe of an intermediate value.</param>
    /// <param name="resultSelector">A function that combines the source value and intermediate value into a result.</param>
    /// <returns>
    /// A Task representing the asynchronous operation, containing a Maybe of the final result.
    /// </returns>
    /// <remarks>
    /// This method enables LINQ query syntax for Maybe types wrapped in Tasks.
    /// It allows for chaining of Maybe-producing operations and flattening of nested Maybes in asynchronous contexts.
    /// The method uses ConfigureAwait(false) to avoid forcing continuations back to the original context.
    /// </remarks>
    /// <example>
    /// Task&lt;Maybe&lt;int&gt;&gt; maybeIntTask = FetchMaybeIntAsync();
    /// var result = await (from x in maybeIntTask
    ///                     from y in GetMaybeString(x)
    ///                     select CombineValues(x, y));
    /// // Where GetMaybeString and CombineValues are synchronous operations.
    /// </example>
    public static async Task<Maybe<TResult>> SelectMany<T, TIntermediate, TResult>(
        this Task<Maybe<T>> source,
        Func<T, Maybe<TIntermediate>> intermediateSelector,
        Func<T, TIntermediate, TResult> resultSelector)
    {
        return (await source.ConfigureAwait(false))
            .SelectMany(intermediateSelector, resultSelector);
    }
}
