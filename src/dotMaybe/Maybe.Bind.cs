using System;

namespace DotMaybe;

public readonly partial record struct Maybe<T>
{
    /// <summary>
    /// Applies a function that returns a Maybe to the value inside this Maybe if it exists.
    /// </summary>
    /// <typeparam name="TResult">The type of the value in the resulting Maybe.</typeparam>
    /// <param name="bind">The function to apply to the value if it exists. This function should return a Maybe.</param>
    /// <returns>
    /// The result of applying the function if this Maybe has a value; otherwise, returns an empty Maybe.
    /// </returns>
    /// <remarks>
    /// This method is useful for chaining operations that may or may not produce a value.
    /// It helps in avoiding nested Maybe instances.
    /// </remarks>
    public Maybe<TResult> Bind<TResult>(Func<T, Maybe<TResult>> bind)
    {
        return Match(Maybe<TResult>.None, bind);
    }
}
