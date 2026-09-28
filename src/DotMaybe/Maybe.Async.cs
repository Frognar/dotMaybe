using static DotMaybe.Prelude;

namespace DotMaybe;

/// <content>
/// Asynchronous counterparts of <c>Match</c>, <c>OrDefault</c>, <c>Map</c>, <c>Bind</c> and <c>Filter</c>.
/// </content>
/// <remarks>
/// <para>
/// The delegates return <see cref="Task{TResult}"/>, so any async lambda or Task-returning method fits.
/// The methods return <see cref="ValueTask{TResult}"/>: when there is nothing to wait for (for example on
/// <see cref="None"/>), the result is already complete and nothing is allocated.
/// </para>
/// <para>
/// Arguments are validated immediately, when the method is called. Anything a delegate does, including throwing
/// before it returns a task, comes out when the result is awaited.
/// To cancel, pass a token through the lambda: <c>maybe.MapAsync(id => repository.GetAsync(id, token))</c>.
/// </para>
/// </remarks>
public readonly partial struct Maybe<T>
    where T : notnull
{
    /// <summary>
    /// Asynchronously reduces this maybe to a single result: <paramref name="some"/> runs on the value,
    /// <paramref name="none"/> runs when there is none. Exactly one of them runs, exactly once.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="some">Produces the result from the value.</param>
    /// <param name="none">Produces the result when there is no value.</param>
    /// <returns>The result of the branch that ran.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="some"/> or <paramref name="none"/> is null.</exception>
    public ValueTask<TResult> MatchAsync<TResult>(Func<T, Task<TResult>> some, Func<Task<TResult>> none)
    {
        ArgumentNullException.ThrowIfNull(some);
        ArgumentNullException.ThrowIfNull(none);
        return _isSome ? Start(some, _value) : Start(none);
    }

    /// <summary>
    /// Asynchronously reduces this maybe to a single result, with a synchronous <paramref name="none"/> branch.
    /// On <see cref="None"/> the result is already complete.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="some">Produces the result from the value.</param>
    /// <param name="none">Produces the result when there is no value.</param>
    /// <returns>The result of the branch that ran.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="some"/> or <paramref name="none"/> is null.</exception>
    public ValueTask<TResult> MatchAsync<TResult>(Func<T, Task<TResult>> some, Func<TResult> none)
    {
        ArgumentNullException.ThrowIfNull(some);
        ArgumentNullException.ThrowIfNull(none);
        return _isSome ? Start(some, _value) : Evaluate(none);
    }

    /// <summary>
    /// Returns the value, or awaits <paramref name="fallback"/> when there is none.
    /// With a value the result is already complete and <paramref name="fallback"/> does not run.
    /// </summary>
    /// <param name="fallback">Produces the result when there is no value.</param>
    /// <returns>The value or the produced fallback.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="fallback"/> is null.</exception>
    public ValueTask<T> OrDefaultAsync(Func<Task<T>> fallback)
    {
        ArgumentNullException.ThrowIfNull(fallback);
        return _isSome ? ValueTask.FromResult(_value) : Start(fallback);
    }

    /// <summary>
    /// Asynchronously transforms the value; <see cref="None"/> stays <see cref="None"/> without waiting.
    /// A <see langword="null"/> result becomes <see cref="None"/>.
    /// </summary>
    /// <typeparam name="TResult">The type of the transformed value.</typeparam>
    /// <param name="map">Transforms the value.</param>
    /// <returns>The transformed value, or <see cref="None"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="map"/> is null.</exception>
    public ValueTask<Maybe<TResult>> MapAsync<TResult>(Func<T, Task<TResult>> map)
        where TResult : notnull
    {
        ArgumentNullException.ThrowIfNull(map);
        return _isSome ? MapValueAsync(_value, map) : ValueTask.FromResult<Maybe<TResult>>(none);

        static async ValueTask<Maybe<TResult>> MapValueAsync(T value, Func<T, Task<TResult>> map) =>
            await map(value).ConfigureAwait(false);
    }

    /// <summary>
    /// Continues with an asynchronous step that may itself produce <see cref="None"/>;
    /// <see cref="None"/> stays <see cref="None"/> without waiting.
    /// </summary>
    /// <typeparam name="TResult">The type of the value produced by the next step.</typeparam>
    /// <param name="bind">The next step.</param>
    /// <returns>The result of the next step, or <see cref="None"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="bind"/> is null.</exception>
    public ValueTask<Maybe<TResult>> BindAsync<TResult>(Func<T, Task<Maybe<TResult>>> bind)
        where TResult : notnull
    {
        ArgumentNullException.ThrowIfNull(bind);
        return _isSome ? Start(bind, _value) : ValueTask.FromResult<Maybe<TResult>>(none);
    }

    /// <summary>
    /// Keeps the value only when the asynchronous <paramref name="predicate"/> holds;
    /// <see cref="None"/> stays <see cref="None"/> without waiting.
    /// </summary>
    /// <param name="predicate">The condition the value must satisfy.</param>
    /// <returns>This maybe when the value satisfies <paramref name="predicate"/>; otherwise <see cref="None"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="predicate"/> is null.</exception>
    public ValueTask<Maybe<T>> FilterAsync(Func<T, Task<bool>> predicate)
    {
        ArgumentNullException.ThrowIfNull(predicate);
        return _isSome ? FilterValueAsync(_value, predicate) : ValueTask.FromResult<Maybe<T>>(none);

        static async ValueTask<Maybe<T>> FilterValueAsync(T value, Func<T, Task<bool>> predicate) =>
            await predicate(value).ConfigureAwait(false) ? value : none;
    }

    private static ValueTask<TResult> Start<TArg, TResult>(Func<TArg, Task<TResult>> step, TArg arg)
    {
        try
        {
            return new ValueTask<TResult>(step(arg));
        }
        catch (OperationCanceledException canceled) when (canceled.CancellationToken.IsCancellationRequested)
        {
            return ValueTask.FromCanceled<TResult>(canceled.CancellationToken);
        }
        catch (Exception error)
        {
            return ValueTask.FromException<TResult>(error);
        }
    }

    private static ValueTask<TResult> Start<TResult>(Func<Task<TResult>> step)
    {
        try
        {
            return new ValueTask<TResult>(step());
        }
        catch (OperationCanceledException canceled) when (canceled.CancellationToken.IsCancellationRequested)
        {
            return ValueTask.FromCanceled<TResult>(canceled.CancellationToken);
        }
        catch (Exception error)
        {
            return ValueTask.FromException<TResult>(error);
        }
    }

    private static ValueTask<TResult> Evaluate<TResult>(Func<TResult> step)
    {
        try
        {
            return ValueTask.FromResult(step());
        }
        catch (OperationCanceledException canceled) when (canceled.CancellationToken.IsCancellationRequested)
        {
            return ValueTask.FromCanceled<TResult>(canceled.CancellationToken);
        }
        catch (Exception error)
        {
            return ValueTask.FromException<TResult>(error);
        }
    }
}
