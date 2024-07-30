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
        return await MapAsync(selector);
    }
}
