namespace DotMaybe;

/// <summary>
/// Query syntax over awaitable maybes: <c>from</c> clauses can await a <see cref="Task{TResult}"/> of a maybe,
/// and the whole query is awaited once.
/// <code>
/// var report = await (
///     from user in repository.FindUserAsync(id)          // Task&lt;Maybe&lt;User&gt;&gt;
///     from order in repository.LastOrderAsync(user)      // Task&lt;Maybe&lt;Order&gt;&gt;
///     where order.Total > 0
///     select $"{user.Name}: {order.Total}");
/// </code>
/// </summary>
/// <remarks>
/// <para>
/// A query can start from a <see cref="Maybe{T}"/>, a <see cref="Task{TResult}"/> or a <see cref="ValueTask{TResult}"/>
/// of a maybe. Every further <c>from</c> may produce a <see cref="Maybe{T}"/> or a <see cref="Task{TResult}"/> of one.
/// The result is a <see cref="ValueTask{TResult}"/> of a maybe.
/// </para>
/// <para>
/// Arguments are validated immediately; a failed or canceled source or step, and anything a delegate does, come out
/// when the query is awaited. Ordering, grouping and joins are not supported.
/// </para>
/// </remarks>
public static class MaybeTaskQueryExtensions
{
    // ---- select ----

    /// <summary>Query-syntax <c>select</c> over an awaitable maybe: awaits the source, then maps the value.</summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <typeparam name="TResult">The type of the selected value.</typeparam>
    /// <param name="source">The awaitable maybe.</param>
    /// <param name="selector">Selects the result from the value.</param>
    /// <returns>The selected value, or <see cref="None"/>.</returns>
    /// <exception cref="ArgumentNullException">An argument is null.</exception>
    public static ValueTask<Maybe<TResult>> Select<T, TResult>(this Task<Maybe<T>> source, Func<T, TResult> selector)
        where T : notnull
        where TResult : notnull
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(selector);
        return source.MapAsync(selector);
    }

    /// <inheritdoc cref="Select{T, TResult}(Task{Maybe{T}}, Func{T, TResult})"/>
    public static ValueTask<Maybe<TResult>> Select<T, TResult>(this ValueTask<Maybe<T>> source, Func<T, TResult> selector)
        where T : notnull
        where TResult : notnull
    {
        ArgumentNullException.ThrowIfNull(selector);
        return source.MapAsync(selector);
    }

    // ---- where ----

    /// <summary>Query-syntax <c>where</c> over an awaitable maybe: awaits the source, then filters the value.</summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="source">The awaitable maybe.</param>
    /// <param name="predicate">The condition the value must satisfy.</param>
    /// <returns>The source maybe when the value satisfies <paramref name="predicate"/>; otherwise <see cref="None"/>.</returns>
    /// <exception cref="ArgumentNullException">An argument is null.</exception>
    public static ValueTask<Maybe<T>> Where<T>(this Task<Maybe<T>> source, Func<T, bool> predicate)
        where T : notnull
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(predicate);
        return source.FilterAsync(predicate);
    }

    /// <inheritdoc cref="Where{T}(Task{Maybe{T}}, Func{T, bool})"/>
    public static ValueTask<Maybe<T>> Where<T>(this ValueTask<Maybe<T>> source, Func<T, bool> predicate)
        where T : notnull
    {
        ArgumentNullException.ThrowIfNull(predicate);
        return source.FilterAsync(predicate);
    }

    // ---- from … from … select ----

    /// <summary>
    /// Query-syntax second <c>from</c> whose step is asynchronous, starting from a plain maybe.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <typeparam name="TIntermediate">The type of the value produced by <paramref name="selector"/>.</typeparam>
    /// <typeparam name="TResult">The type of the projected value.</typeparam>
    /// <param name="source">The maybe.</param>
    /// <param name="selector">The next, asynchronous step.</param>
    /// <param name="resultSelector">Combines the value with the result of the next step.</param>
    /// <returns>The projected value, or <see cref="None"/> when either step has no value.</returns>
    /// <exception cref="ArgumentNullException">An argument is null.</exception>
    public static ValueTask<Maybe<TResult>> SelectMany<T, TIntermediate, TResult>(
        this Maybe<T> source,
        Func<T, Task<Maybe<TIntermediate>>> selector,
        Func<T, TIntermediate, TResult> resultSelector)
        where T : notnull
        where TIntermediate : notnull
        where TResult : notnull
    {
        ArgumentNullException.ThrowIfNull(selector);
        ArgumentNullException.ThrowIfNull(resultSelector);
        return source.BindAsync(async v1 =>
            (await selector(v1).ConfigureAwait(false)).Map(v2 => resultSelector(v1, v2)));
    }

    /// <summary>
    /// Query-syntax second <c>from</c> over an awaitable maybe, with a synchronous step.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <typeparam name="TIntermediate">The type of the value produced by <paramref name="selector"/>.</typeparam>
    /// <typeparam name="TResult">The type of the projected value.</typeparam>
    /// <param name="source">The awaitable maybe.</param>
    /// <param name="selector">The next step.</param>
    /// <param name="resultSelector">Combines the value with the result of the next step.</param>
    /// <returns>The projected value, or <see cref="None"/> when either step has no value.</returns>
    /// <exception cref="ArgumentNullException">An argument is null.</exception>
    public static ValueTask<Maybe<TResult>> SelectMany<T, TIntermediate, TResult>(
        this Task<Maybe<T>> source,
        Func<T, Maybe<TIntermediate>> selector,
        Func<T, TIntermediate, TResult> resultSelector)
        where T : notnull
        where TIntermediate : notnull
        where TResult : notnull
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(selector);
        ArgumentNullException.ThrowIfNull(resultSelector);
        return source.BindAsync(v1 => selector(v1).Map(v2 => resultSelector(v1, v2)));
    }

    /// <summary>
    /// Query-syntax second <c>from</c> over an awaitable maybe, with an asynchronous step.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <typeparam name="TIntermediate">The type of the value produced by <paramref name="selector"/>.</typeparam>
    /// <typeparam name="TResult">The type of the projected value.</typeparam>
    /// <param name="source">The awaitable maybe.</param>
    /// <param name="selector">The next, asynchronous step.</param>
    /// <param name="resultSelector">Combines the value with the result of the next step.</param>
    /// <returns>The projected value, or <see cref="None"/> when either step has no value.</returns>
    /// <exception cref="ArgumentNullException">An argument is null.</exception>
    public static ValueTask<Maybe<TResult>> SelectMany<T, TIntermediate, TResult>(
        this Task<Maybe<T>> source,
        Func<T, Task<Maybe<TIntermediate>>> selector,
        Func<T, TIntermediate, TResult> resultSelector)
        where T : notnull
        where TIntermediate : notnull
        where TResult : notnull
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(selector);
        ArgumentNullException.ThrowIfNull(resultSelector);
        return source.BindAsync(async v1 =>
            (await selector(v1).ConfigureAwait(false)).Map(v2 => resultSelector(v1, v2)));
    }

    /// <inheritdoc cref="SelectMany{T, TIntermediate, TResult}(Task{Maybe{T}}, Func{T, Maybe{TIntermediate}}, Func{T, TIntermediate, TResult})"/>
    public static ValueTask<Maybe<TResult>> SelectMany<T, TIntermediate, TResult>(
        this ValueTask<Maybe<T>> source,
        Func<T, Maybe<TIntermediate>> selector,
        Func<T, TIntermediate, TResult> resultSelector)
        where T : notnull
        where TIntermediate : notnull
        where TResult : notnull
    {
        ArgumentNullException.ThrowIfNull(selector);
        ArgumentNullException.ThrowIfNull(resultSelector);
        return source.BindAsync(v1 => selector(v1).Map(v2 => resultSelector(v1, v2)));
    }

    /// <inheritdoc cref="SelectMany{T, TIntermediate, TResult}(Task{Maybe{T}}, Func{T, Task{Maybe{TIntermediate}}}, Func{T, TIntermediate, TResult})"/>
    public static ValueTask<Maybe<TResult>> SelectMany<T, TIntermediate, TResult>(
        this ValueTask<Maybe<T>> source,
        Func<T, Task<Maybe<TIntermediate>>> selector,
        Func<T, TIntermediate, TResult> resultSelector)
        where T : notnull
        where TIntermediate : notnull
        where TResult : notnull
    {
        ArgumentNullException.ThrowIfNull(selector);
        ArgumentNullException.ThrowIfNull(resultSelector);
        return source.BindAsync(async v1 =>
            (await selector(v1).ConfigureAwait(false)).Map(v2 => resultSelector(v1, v2)));
    }
}
