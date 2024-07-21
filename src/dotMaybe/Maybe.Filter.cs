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
    public Maybe<T> Filter(Func<T, bool> predicate)
    {
        return _maybe switch
        {
            NoneType => this,
            SomeType some when predicate(some.Value) => this,
            _ => None(),
        };
    }
}
