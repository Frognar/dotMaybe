using static DotMaybe.Prelude;

namespace DotMaybe;

/// <content>
/// Combinators: <c>OrElse</c>, <c>Zip</c>, <c>Fold</c>, <c>Iter</c> and <c>Tap</c>.
/// </content>
public readonly partial struct Maybe<T>
{
    /// <summary>
    /// Returns this maybe when it has a value; otherwise <paramref name="alternative"/>.
    /// </summary>
    /// <remarks>
    /// Unlike <see cref="OrDefault(T)"/>, the result is still a maybe, so alternatives can be chained:
    /// <c>fromCache.OrElse(fromConfig).OrElse(fromDefaults)</c>.
    /// </remarks>
    /// <param name="alternative">The maybe to use when this one has no value.</param>
    /// <returns>This maybe, or <paramref name="alternative"/>.</returns>
    public Maybe<T> OrElse(Maybe<T> alternative) => _isSome ? this : alternative;

    /// <summary>
    /// Returns this maybe when it has a value; otherwise the result of <paramref name="alternative"/>.
    /// <paramref name="alternative"/> runs only when there is no value.
    /// </summary>
    /// <param name="alternative">Produces the maybe to use when this one has no value.</param>
    /// <returns>This maybe, or the produced alternative.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="alternative"/> is null.</exception>
    public Maybe<T> OrElse(Func<Maybe<T>> alternative)
    {
        ArgumentNullException.ThrowIfNull(alternative);
        return _isSome ? this : alternative();
    }

    /// <summary>
    /// Pairs this value with the value of <paramref name="other"/>; <see cref="None"/> when either has none.
    /// </summary>
    /// <typeparam name="TOther">The type of the other value.</typeparam>
    /// <param name="other">The other maybe.</param>
    /// <returns>Both values as a pair, or <see cref="None"/>.</returns>
    public Maybe<(T, TOther)> Zip<TOther>(Maybe<TOther> other)
        where TOther : notnull
    {
        if (!_isSome || !other._isSome)
        {
            return none;
        }

        return (_value, other._value);
    }

    /// <summary>
    /// Combines this value with the value of <paramref name="other"/>; <see cref="None"/> when either has none.
    /// </summary>
    /// <remarks>
    /// <paramref name="combine"/> runs only when both have a value. A <see langword="null"/> result becomes
    /// <see cref="None"/>. Unlike <see cref="Bind{TResult}"/>, the two maybes do not depend on each other.
    /// </remarks>
    /// <typeparam name="TOther">The type of the other value.</typeparam>
    /// <typeparam name="TResult">The type of the combined value.</typeparam>
    /// <param name="other">The other maybe.</param>
    /// <param name="combine">Combines both values.</param>
    /// <returns>The combined value, or <see cref="None"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="combine"/> is null.</exception>
    public Maybe<TResult> Zip<TOther, TResult>(Maybe<TOther> other, Func<T, TOther, TResult> combine)
        where TOther : notnull
        where TResult : notnull
    {
        ArgumentNullException.ThrowIfNull(combine);
        if (!_isSome || !other._isSome)
        {
            return none;
        }

        return combine(_value, other._value);
    }

    /// <summary>
    /// Folds the value into <paramref name="state"/> with <paramref name="folder"/>; without a value, returns
    /// <paramref name="state"/> unchanged.
    /// </summary>
    /// <remarks>
    /// <paramref name="folder"/> runs only when there is a value. The state is not validated and may be
    /// <see langword="null"/> when <typeparamref name="TState"/> allows it.
    /// </remarks>
    /// <typeparam name="TState">The type of the state.</typeparam>
    /// <param name="state">The initial state.</param>
    /// <param name="folder">Combines the state with the value.</param>
    /// <returns>The folded state, or <paramref name="state"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="folder"/> is null.</exception>
    public TState Fold<TState>(TState state, Func<TState, T, TState> folder)
    {
        ArgumentNullException.ThrowIfNull(folder);
        return _isSome ? folder(state, _value) : state;
    }

    /// <summary>
    /// Runs <paramref name="action"/> on the value, for its side effect. Does nothing without a value.
    /// </summary>
    /// <param name="action">The side effect to run on the value.</param>
    /// <exception cref="ArgumentNullException"><paramref name="action"/> is null.</exception>
    public void Iter(Action<T> action)
    {
        ArgumentNullException.ThrowIfNull(action);
        if (_isSome)
        {
            action(_value);
        }
    }

    /// <summary>
    /// Runs <paramref name="action"/> on the value, for its side effect, and returns this maybe unchanged,
    /// so the chain goes on: <c>maybe.Tap(log).Map(…)</c>. Does nothing without a value.
    /// </summary>
    /// <remarks>
    /// Use <see cref="Iter"/> when the side effect ends the chain.
    /// </remarks>
    /// <param name="action">The side effect to run on the value.</param>
    /// <returns>This maybe.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="action"/> is null.</exception>
    public Maybe<T> Tap(Action<T> action)
    {
        ArgumentNullException.ThrowIfNull(action);
        if (_isSome)
        {
            action(_value);
        }

        return this;
    }
}
