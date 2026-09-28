namespace DotMaybe;

/// <summary>
/// Continues a maybe pipeline on an awaitable maybe (<see cref="Task{TResult}"/> or <see cref="ValueTask{TResult}"/>
/// of <see cref="Maybe{T}"/>), so async steps chain without an <c>await</c> after each one:
/// <code>
/// var name = await repository.FindAsync(id)
///     .MapAsync(user => user.Name)
///     .FilterAsync(name => name.Length > 0)
///     .OrDefaultAsync("anonymous");
/// </code>
/// </summary>
/// <remarks>
/// <para>
/// Each method awaits the source and then does what its synchronous or asynchronous counterpart on
/// <see cref="Maybe{T}"/> does. Every overload takes either a synchronous delegate or one that returns a
/// <see cref="Task{TResult}"/>; async lambdas pick the asynchronous overload.
/// </para>
/// <para>
/// Arguments are validated immediately. A failed or canceled source, and anything a delegate does, come out when
/// the result is awaited.
/// </para>
/// </remarks>
public static class MaybeTaskExtensions
{
    // ---- Task<Maybe<T>> sources ----

    /// <summary>Awaits the source, then transforms the value with <paramref name="map"/>.</summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <typeparam name="TResult">The type of the transformed value.</typeparam>
    /// <param name="source">The awaitable maybe.</param>
    /// <param name="map">Transforms the value.</param>
    /// <returns>The transformed value, or <see cref="None"/>.</returns>
    /// <exception cref="ArgumentNullException">An argument is null.</exception>
    public static ValueTask<Maybe<TResult>> MapAsync<T, TResult>(this Task<Maybe<T>> source, Func<T, TResult> map)
        where T : notnull
        where TResult : notnull
    {
        ArgumentNullException.ThrowIfNull(source);
        return new ValueTask<Maybe<T>>(source).MapAsync(map);
    }

    /// <summary>Awaits the source, then transforms the value with the asynchronous <paramref name="map"/>.</summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <typeparam name="TResult">The type of the transformed value.</typeparam>
    /// <param name="source">The awaitable maybe.</param>
    /// <param name="map">Transforms the value.</param>
    /// <returns>The transformed value, or <see cref="None"/>.</returns>
    /// <exception cref="ArgumentNullException">An argument is null.</exception>
    public static ValueTask<Maybe<TResult>> MapAsync<T, TResult>(this Task<Maybe<T>> source, Func<T, Task<TResult>> map)
        where T : notnull
        where TResult : notnull
    {
        ArgumentNullException.ThrowIfNull(source);
        return new ValueTask<Maybe<T>>(source).MapAsync(map);
    }

    /// <summary>Awaits the source, then continues with <paramref name="bind"/>.</summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <typeparam name="TResult">The type of the value produced by the next step.</typeparam>
    /// <param name="source">The awaitable maybe.</param>
    /// <param name="bind">The next step.</param>
    /// <returns>The result of the next step, or <see cref="None"/>.</returns>
    /// <exception cref="ArgumentNullException">An argument is null.</exception>
    public static ValueTask<Maybe<TResult>> BindAsync<T, TResult>(this Task<Maybe<T>> source, Func<T, Maybe<TResult>> bind)
        where T : notnull
        where TResult : notnull
    {
        ArgumentNullException.ThrowIfNull(source);
        return new ValueTask<Maybe<T>>(source).BindAsync(bind);
    }

    /// <summary>Awaits the source, then continues with the asynchronous <paramref name="bind"/>.</summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <typeparam name="TResult">The type of the value produced by the next step.</typeparam>
    /// <param name="source">The awaitable maybe.</param>
    /// <param name="bind">The next step.</param>
    /// <returns>The result of the next step, or <see cref="None"/>.</returns>
    /// <exception cref="ArgumentNullException">An argument is null.</exception>
    public static ValueTask<Maybe<TResult>> BindAsync<T, TResult>(
        this Task<Maybe<T>> source,
        Func<T, Task<Maybe<TResult>>> bind)
        where T : notnull
        where TResult : notnull
    {
        ArgumentNullException.ThrowIfNull(source);
        return new ValueTask<Maybe<T>>(source).BindAsync(bind);
    }

    /// <summary>Awaits the source, then keeps the value only when <paramref name="predicate"/> holds.</summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="source">The awaitable maybe.</param>
    /// <param name="predicate">The condition the value must satisfy.</param>
    /// <returns>The source maybe when the value satisfies <paramref name="predicate"/>; otherwise <see cref="None"/>.</returns>
    /// <exception cref="ArgumentNullException">An argument is null.</exception>
    public static ValueTask<Maybe<T>> FilterAsync<T>(this Task<Maybe<T>> source, Func<T, bool> predicate)
        where T : notnull
    {
        ArgumentNullException.ThrowIfNull(source);
        return new ValueTask<Maybe<T>>(source).FilterAsync(predicate);
    }

    /// <summary>Awaits the source, then keeps the value only when the asynchronous <paramref name="predicate"/> holds.</summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="source">The awaitable maybe.</param>
    /// <param name="predicate">The condition the value must satisfy.</param>
    /// <returns>The source maybe when the value satisfies <paramref name="predicate"/>; otherwise <see cref="None"/>.</returns>
    /// <exception cref="ArgumentNullException">An argument is null.</exception>
    public static ValueTask<Maybe<T>> FilterAsync<T>(this Task<Maybe<T>> source, Func<T, Task<bool>> predicate)
        where T : notnull
    {
        ArgumentNullException.ThrowIfNull(source);
        return new ValueTask<Maybe<T>>(source).FilterAsync(predicate);
    }

    /// <summary>Awaits the source, then reduces it with synchronous branches.</summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="source">The awaitable maybe.</param>
    /// <param name="some">Produces the result from the value.</param>
    /// <param name="none">Produces the result when there is no value.</param>
    /// <returns>The result of the branch that ran.</returns>
    /// <exception cref="ArgumentNullException">An argument is null.</exception>
    public static ValueTask<TResult> MatchAsync<T, TResult>(this Task<Maybe<T>> source, Func<T, TResult> some, Func<TResult> none)
        where T : notnull
    {
        ArgumentNullException.ThrowIfNull(source);
        return new ValueTask<Maybe<T>>(source).MatchAsync(some, none);
    }

    /// <summary>Awaits the source, then reduces it with asynchronous branches.</summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="source">The awaitable maybe.</param>
    /// <param name="some">Produces the result from the value.</param>
    /// <param name="none">Produces the result when there is no value.</param>
    /// <returns>The result of the branch that ran.</returns>
    /// <exception cref="ArgumentNullException">An argument is null.</exception>
    public static ValueTask<TResult> MatchAsync<T, TResult>(
        this Task<Maybe<T>> source,
        Func<T, Task<TResult>> some,
        Func<Task<TResult>> none)
        where T : notnull
    {
        ArgumentNullException.ThrowIfNull(source);
        return new ValueTask<Maybe<T>>(source).MatchAsync(some, none);
    }

    /// <summary>Awaits the source, then reduces it with an asynchronous <paramref name="some"/> and a synchronous <paramref name="none"/>.</summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="source">The awaitable maybe.</param>
    /// <param name="some">Produces the result from the value.</param>
    /// <param name="none">Produces the result when there is no value.</param>
    /// <returns>The result of the branch that ran.</returns>
    /// <exception cref="ArgumentNullException">An argument is null.</exception>
    public static ValueTask<TResult> MatchAsync<T, TResult>(
        this Task<Maybe<T>> source,
        Func<T, Task<TResult>> some,
        Func<TResult> none)
        where T : notnull
    {
        ArgumentNullException.ThrowIfNull(source);
        return new ValueTask<Maybe<T>>(source).MatchAsync(some, none);
    }

    /// <summary>Awaits the source, then returns the value or <paramref name="fallback"/>.</summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="source">The awaitable maybe.</param>
    /// <param name="fallback">The result when there is no value.</param>
    /// <returns>The value or the fallback.</returns>
    /// <exception cref="ArgumentNullException">An argument is null.</exception>
    public static ValueTask<T> OrDefaultAsync<T>(this Task<Maybe<T>> source, T fallback)
        where T : notnull
    {
        ArgumentNullException.ThrowIfNull(source);
        return new ValueTask<Maybe<T>>(source).OrDefaultAsync(fallback);
    }

    /// <summary>Awaits the source, then returns the value or the result of <paramref name="fallback"/>.</summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="source">The awaitable maybe.</param>
    /// <param name="fallback">Produces the result when there is no value.</param>
    /// <returns>The value or the produced fallback.</returns>
    /// <exception cref="ArgumentNullException">An argument is null.</exception>
    public static ValueTask<T> OrDefaultAsync<T>(this Task<Maybe<T>> source, Func<T> fallback)
        where T : notnull
    {
        ArgumentNullException.ThrowIfNull(source);
        return new ValueTask<Maybe<T>>(source).OrDefaultAsync(fallback);
    }

    /// <summary>Awaits the source, then returns the value or awaits <paramref name="fallback"/>.</summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="source">The awaitable maybe.</param>
    /// <param name="fallback">Produces the result when there is no value.</param>
    /// <returns>The value or the produced fallback.</returns>
    /// <exception cref="ArgumentNullException">An argument is null.</exception>
    public static ValueTask<T> OrDefaultAsync<T>(this Task<Maybe<T>> source, Func<Task<T>> fallback)
        where T : notnull
    {
        ArgumentNullException.ThrowIfNull(source);
        return new ValueTask<Maybe<T>>(source).OrDefaultAsync(fallback);
    }

    // ---- ValueTask<Maybe<T>> sources ----

    /// <summary>Awaits the source, then transforms the value with <paramref name="map"/>.</summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <typeparam name="TResult">The type of the transformed value.</typeparam>
    /// <param name="source">The awaitable maybe.</param>
    /// <param name="map">Transforms the value.</param>
    /// <returns>The transformed value, or <see cref="None"/>.</returns>
    /// <exception cref="ArgumentNullException">An argument is null.</exception>
    public static ValueTask<Maybe<TResult>> MapAsync<T, TResult>(this ValueTask<Maybe<T>> source, Func<T, TResult> map)
        where T : notnull
        where TResult : notnull
    {
        ArgumentNullException.ThrowIfNull(map);
        return MapAfter(source, map);

        static async ValueTask<Maybe<TResult>> MapAfter(ValueTask<Maybe<T>> source, Func<T, TResult> map) =>
            (await source.ConfigureAwait(false)).Map(map);
    }

    /// <summary>Awaits the source, then transforms the value with the asynchronous <paramref name="map"/>.</summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <typeparam name="TResult">The type of the transformed value.</typeparam>
    /// <param name="source">The awaitable maybe.</param>
    /// <param name="map">Transforms the value.</param>
    /// <returns>The transformed value, or <see cref="None"/>.</returns>
    /// <exception cref="ArgumentNullException">An argument is null.</exception>
    public static ValueTask<Maybe<TResult>> MapAsync<T, TResult>(this ValueTask<Maybe<T>> source, Func<T, Task<TResult>> map)
        where T : notnull
        where TResult : notnull
    {
        ArgumentNullException.ThrowIfNull(map);
        return MapAfter(source, map);

        static async ValueTask<Maybe<TResult>> MapAfter(ValueTask<Maybe<T>> source, Func<T, Task<TResult>> map) =>
            await (await source.ConfigureAwait(false)).MapAsync(map).ConfigureAwait(false);
    }

    /// <summary>Awaits the source, then continues with <paramref name="bind"/>.</summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <typeparam name="TResult">The type of the value produced by the next step.</typeparam>
    /// <param name="source">The awaitable maybe.</param>
    /// <param name="bind">The next step.</param>
    /// <returns>The result of the next step, or <see cref="None"/>.</returns>
    /// <exception cref="ArgumentNullException">An argument is null.</exception>
    public static ValueTask<Maybe<TResult>> BindAsync<T, TResult>(this ValueTask<Maybe<T>> source, Func<T, Maybe<TResult>> bind)
        where T : notnull
        where TResult : notnull
    {
        ArgumentNullException.ThrowIfNull(bind);
        return BindAfter(source, bind);

        static async ValueTask<Maybe<TResult>> BindAfter(ValueTask<Maybe<T>> source, Func<T, Maybe<TResult>> bind) =>
            (await source.ConfigureAwait(false)).Bind(bind);
    }

    /// <summary>Awaits the source, then continues with the asynchronous <paramref name="bind"/>.</summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <typeparam name="TResult">The type of the value produced by the next step.</typeparam>
    /// <param name="source">The awaitable maybe.</param>
    /// <param name="bind">The next step.</param>
    /// <returns>The result of the next step, or <see cref="None"/>.</returns>
    /// <exception cref="ArgumentNullException">An argument is null.</exception>
    public static ValueTask<Maybe<TResult>> BindAsync<T, TResult>(
        this ValueTask<Maybe<T>> source,
        Func<T, Task<Maybe<TResult>>> bind)
        where T : notnull
        where TResult : notnull
    {
        ArgumentNullException.ThrowIfNull(bind);
        return BindAfter(source, bind);

        static async ValueTask<Maybe<TResult>> BindAfter(ValueTask<Maybe<T>> source, Func<T, Task<Maybe<TResult>>> bind) =>
            await (await source.ConfigureAwait(false)).BindAsync(bind).ConfigureAwait(false);
    }

    /// <summary>Awaits the source, then keeps the value only when <paramref name="predicate"/> holds.</summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="source">The awaitable maybe.</param>
    /// <param name="predicate">The condition the value must satisfy.</param>
    /// <returns>The source maybe when the value satisfies <paramref name="predicate"/>; otherwise <see cref="None"/>.</returns>
    /// <exception cref="ArgumentNullException">An argument is null.</exception>
    public static ValueTask<Maybe<T>> FilterAsync<T>(this ValueTask<Maybe<T>> source, Func<T, bool> predicate)
        where T : notnull
    {
        ArgumentNullException.ThrowIfNull(predicate);
        return FilterAfter(source, predicate);

        static async ValueTask<Maybe<T>> FilterAfter(ValueTask<Maybe<T>> source, Func<T, bool> predicate) =>
            (await source.ConfigureAwait(false)).Filter(predicate);
    }

    /// <summary>Awaits the source, then keeps the value only when the asynchronous <paramref name="predicate"/> holds.</summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="source">The awaitable maybe.</param>
    /// <param name="predicate">The condition the value must satisfy.</param>
    /// <returns>The source maybe when the value satisfies <paramref name="predicate"/>; otherwise <see cref="None"/>.</returns>
    /// <exception cref="ArgumentNullException">An argument is null.</exception>
    public static ValueTask<Maybe<T>> FilterAsync<T>(this ValueTask<Maybe<T>> source, Func<T, Task<bool>> predicate)
        where T : notnull
    {
        ArgumentNullException.ThrowIfNull(predicate);
        return FilterAfter(source, predicate);

        static async ValueTask<Maybe<T>> FilterAfter(ValueTask<Maybe<T>> source, Func<T, Task<bool>> predicate) =>
            await (await source.ConfigureAwait(false)).FilterAsync(predicate).ConfigureAwait(false);
    }

    /// <summary>Awaits the source, then reduces it with synchronous branches.</summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="source">The awaitable maybe.</param>
    /// <param name="some">Produces the result from the value.</param>
    /// <param name="none">Produces the result when there is no value.</param>
    /// <returns>The result of the branch that ran.</returns>
    /// <exception cref="ArgumentNullException">An argument is null.</exception>
    public static ValueTask<TResult> MatchAsync<T, TResult>(this ValueTask<Maybe<T>> source, Func<T, TResult> some, Func<TResult> none)
        where T : notnull
    {
        ArgumentNullException.ThrowIfNull(some);
        ArgumentNullException.ThrowIfNull(none);
        return MatchAfter(source, some, none);

        static async ValueTask<TResult> MatchAfter(ValueTask<Maybe<T>> source, Func<T, TResult> some, Func<TResult> none) =>
            (await source.ConfigureAwait(false)).Match(some, none);
    }

    /// <summary>Awaits the source, then reduces it with asynchronous branches.</summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="source">The awaitable maybe.</param>
    /// <param name="some">Produces the result from the value.</param>
    /// <param name="none">Produces the result when there is no value.</param>
    /// <returns>The result of the branch that ran.</returns>
    /// <exception cref="ArgumentNullException">An argument is null.</exception>
    public static ValueTask<TResult> MatchAsync<T, TResult>(
        this ValueTask<Maybe<T>> source,
        Func<T, Task<TResult>> some,
        Func<Task<TResult>> none)
        where T : notnull
    {
        ArgumentNullException.ThrowIfNull(some);
        ArgumentNullException.ThrowIfNull(none);
        return MatchAfter(source, some, none);

        static async ValueTask<TResult> MatchAfter(ValueTask<Maybe<T>> source, Func<T, Task<TResult>> some, Func<Task<TResult>> none) =>
            await (await source.ConfigureAwait(false)).MatchAsync(some, none).ConfigureAwait(false);
    }

    /// <summary>Awaits the source, then reduces it with an asynchronous <paramref name="some"/> and a synchronous <paramref name="none"/>.</summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="source">The awaitable maybe.</param>
    /// <param name="some">Produces the result from the value.</param>
    /// <param name="none">Produces the result when there is no value.</param>
    /// <returns>The result of the branch that ran.</returns>
    /// <exception cref="ArgumentNullException">An argument is null.</exception>
    public static ValueTask<TResult> MatchAsync<T, TResult>(
        this ValueTask<Maybe<T>> source,
        Func<T, Task<TResult>> some,
        Func<TResult> none)
        where T : notnull
    {
        ArgumentNullException.ThrowIfNull(some);
        ArgumentNullException.ThrowIfNull(none);
        return MatchAfter(source, some, none);

        static async ValueTask<TResult> MatchAfter(ValueTask<Maybe<T>> source, Func<T, Task<TResult>> some, Func<TResult> none) =>
            await (await source.ConfigureAwait(false)).MatchAsync(some, none).ConfigureAwait(false);
    }

    /// <summary>Awaits the source, then returns the value or <paramref name="fallback"/>.</summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="source">The awaitable maybe.</param>
    /// <param name="fallback">The result when there is no value.</param>
    /// <returns>The value or the fallback.</returns>
    /// <exception cref="ArgumentNullException">An argument is null.</exception>
    public static ValueTask<T> OrDefaultAsync<T>(this ValueTask<Maybe<T>> source, T fallback)
        where T : notnull
    {
        ArgumentNullException.ThrowIfNull(fallback);
        return OrDefaultAfter(source, fallback);

        static async ValueTask<T> OrDefaultAfter(ValueTask<Maybe<T>> source, T fallback) =>
            (await source.ConfigureAwait(false)).OrDefault(fallback);
    }

    /// <summary>Awaits the source, then returns the value or the result of <paramref name="fallback"/>.</summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="source">The awaitable maybe.</param>
    /// <param name="fallback">Produces the result when there is no value.</param>
    /// <returns>The value or the produced fallback.</returns>
    /// <exception cref="ArgumentNullException">An argument is null.</exception>
    public static ValueTask<T> OrDefaultAsync<T>(this ValueTask<Maybe<T>> source, Func<T> fallback)
        where T : notnull
    {
        ArgumentNullException.ThrowIfNull(fallback);
        return OrDefaultAfter(source, fallback);

        static async ValueTask<T> OrDefaultAfter(ValueTask<Maybe<T>> source, Func<T> fallback) =>
            (await source.ConfigureAwait(false)).OrDefault(fallback);
    }

    /// <summary>Awaits the source, then returns the value or awaits <paramref name="fallback"/>.</summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="source">The awaitable maybe.</param>
    /// <param name="fallback">Produces the result when there is no value.</param>
    /// <returns>The value or the produced fallback.</returns>
    /// <exception cref="ArgumentNullException">An argument is null.</exception>
    public static ValueTask<T> OrDefaultAsync<T>(this ValueTask<Maybe<T>> source, Func<Task<T>> fallback)
        where T : notnull
    {
        ArgumentNullException.ThrowIfNull(fallback);
        return OrDefaultAfter(source, fallback);

        static async ValueTask<T> OrDefaultAfter(ValueTask<Maybe<T>> source, Func<Task<T>> fallback) =>
            await (await source.ConfigureAwait(false)).OrDefaultAsync(fallback).ConfigureAwait(false);
    }
}
