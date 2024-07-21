using System;

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
    public Maybe<T> Where(Func<T, bool> predicate)
    {
        return Filter(predicate);
    }
}
