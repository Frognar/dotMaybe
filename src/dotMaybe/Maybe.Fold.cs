using System;

namespace DotMaybe;

public readonly partial record struct Maybe<T>
{
    /// <summary>
    /// Applies an aggregation function to the value contained in Maybe, if it exists.
    /// </summary>
    /// <typeparam name="TState">The type of the state and aggregation result.</typeparam>
    /// <param name="state">The initial state of the aggregation.</param>
    /// <param name="folder">The aggregation function that takes the current state and value, returning a new state.</param>
    /// <returns>
    /// If Maybe contains a value (Some), returns the result of applying the folder function to that value and the initial state.
    /// If Maybe does not contain a value (None), returns the initial state without modification.
    /// </returns>
    /// <remarks>
    /// This method is useful for safely processing potentially non-existent values
    /// without the need for manual existence checks.
    /// </remarks>
    public TState Fold<TState>(TState state, Func<TState, T, TState> folder)
    {
        return Match(() => state, value => folder(state, value));
    }
}
