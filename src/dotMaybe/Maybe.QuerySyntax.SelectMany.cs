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
    public async Task<Maybe<TResult>> SelectMany<TIntermediate, TResult>(
        Func<T, Maybe<TIntermediate>> intermediateSelector,
        Func<T, TIntermediate, Task<TResult>> resultSelector)
    {
        return await BindAsync(v1 => intermediateSelector(v1)
                .MapAsync(async v2 => await resultSelector(v1, v2).ConfigureAwait(false)))
            .ConfigureAwait(false);
    }
}
