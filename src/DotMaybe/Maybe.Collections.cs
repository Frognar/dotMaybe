using static DotMaybe.Prelude;

namespace DotMaybe;

/// <content>
/// Maybes and sequences: picking values out (<c>Choose</c>), all-or-nothing (<c>Sequence</c>, <c>Traverse</c>),
/// and lookups that may find nothing (<c>FirstOrNone</c>, <c>SingleOrNone</c>, <c>GetValueOrNone</c>).
/// </content>
/// <remarks>
/// Arguments are validated immediately. <c>Choose</c> is deferred like LINQ; the other methods run at once and stop
/// reading the sequence as soon as the answer is known.
/// </remarks>
public static partial class Maybe
{
    // ---- Choose ----

    /// <summary>
    /// The values of the maybes that have one, in order; maybes without a value are skipped.
    /// </summary>
    /// <remarks>Deferred: nothing is read until the result is enumerated.</remarks>
    /// <typeparam name="T">The type of the values.</typeparam>
    /// <param name="source">The maybes.</param>
    /// <returns>The values.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> is null.</exception>
    public static IEnumerable<T> Choose<T>(this IEnumerable<Maybe<T>> source)
        where T : notnull
    {
        ArgumentNullException.ThrowIfNull(source);
        return Values(source);

        static IEnumerable<T> Values(IEnumerable<Maybe<T>> source)
        {
            foreach (var maybe in source)
            {
                if (maybe.TryGetSome(out var value))
                {
                    yield return value;
                }
            }
        }
    }

    /// <summary>
    /// Applies <paramref name="chooser"/> to every element and keeps the values it produces, in order.
    /// </summary>
    /// <remarks>Deferred: nothing is read and <paramref name="chooser"/> does not run until the result is enumerated.</remarks>
    /// <typeparam name="T">The type of the elements.</typeparam>
    /// <typeparam name="TResult">The type of the chosen values.</typeparam>
    /// <param name="source">The elements.</param>
    /// <param name="chooser">Produces a value, or <see cref="None"/> to skip the element.</param>
    /// <returns>The chosen values.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="chooser"/> is null.</exception>
    public static IEnumerable<TResult> Choose<T, TResult>(this IEnumerable<T> source, Func<T, Maybe<TResult>> chooser)
        where TResult : notnull
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(chooser);
        return source.Select(chooser).Choose();
    }

    // ---- Sequence / Traverse ----

    /// <summary>
    /// All the values, when every maybe has one; otherwise <see cref="None"/>.
    /// An empty sequence gives an empty list.
    /// </summary>
    /// <remarks>Reading stops at the first maybe without a value.</remarks>
    /// <typeparam name="T">The type of the values.</typeparam>
    /// <param name="source">The maybes.</param>
    /// <returns>The values in order, or <see cref="None"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> is null.</exception>
    public static Maybe<IReadOnlyList<T>> Sequence<T>(this IEnumerable<Maybe<T>> source)
        where T : notnull
    {
        ArgumentNullException.ThrowIfNull(source);
        var values = source.TryGetNonEnumeratedCount(out var count) ? new List<T>(count) : [];
        foreach (var maybe in source)
        {
            if (!maybe.TryGetSome(out var value))
            {
                return none;
            }

            values.Add(value);
        }

        return values;
    }

    /// <summary>
    /// Applies <paramref name="selector"/> to every element: all the results when every one has a value;
    /// otherwise <see cref="None"/>. An empty sequence gives an empty list.
    /// </summary>
    /// <remarks>
    /// Reading stops, and <paramref name="selector"/> is no longer called, at the first result without a value.
    /// </remarks>
    /// <typeparam name="T">The type of the elements.</typeparam>
    /// <typeparam name="TResult">The type of the results.</typeparam>
    /// <param name="source">The elements.</param>
    /// <param name="selector">Produces a value, or <see cref="None"/> to fail the whole traversal.</param>
    /// <returns>The results in order, or <see cref="None"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="selector"/> is null.</exception>
    public static Maybe<IReadOnlyList<TResult>> Traverse<T, TResult>(
        this IEnumerable<T> source,
        Func<T, Maybe<TResult>> selector)
        where TResult : notnull
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(selector);
        return source.Select(selector).Sequence();
    }

    // ---- FirstOrNone / SingleOrNone ----

    /// <summary>The first element, or <see cref="None"/> when the sequence is empty.</summary>
    /// <remarks>Reads one element at most. A <see langword="null"/> first element gives <see cref="None"/>.</remarks>
    /// <typeparam name="T">The type of the elements.</typeparam>
    /// <param name="source">The elements.</param>
    /// <returns>The first element, or <see cref="None"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> is null.</exception>
    public static Maybe<T> FirstOrNone<T>(this IEnumerable<T> source)
        where T : notnull
    {
        ArgumentNullException.ThrowIfNull(source);
        foreach (var value in source)
        {
            return value;
        }

        return none;
    }

    /// <summary>The first element that satisfies <paramref name="predicate"/>, or <see cref="None"/>.</summary>
    /// <remarks>Stops reading at the first match.</remarks>
    /// <typeparam name="T">The type of the elements.</typeparam>
    /// <param name="source">The elements.</param>
    /// <param name="predicate">The condition to satisfy.</param>
    /// <returns>The first matching element, or <see cref="None"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="predicate"/> is null.</exception>
    public static Maybe<T> FirstOrNone<T>(this IEnumerable<T> source, Func<T, bool> predicate)
        where T : notnull
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(predicate);
        foreach (var value in source)
        {
            if (predicate(value))
            {
                return value;
            }
        }

        return none;
    }

    /// <summary>
    /// The only element, or <see cref="None"/> when the sequence does not have exactly one element.
    /// </summary>
    /// <remarks>
    /// Unlike <see cref="Enumerable.SingleOrDefault{TSource}(IEnumerable{TSource})"/>, more than one element is not an
    /// error: it gives <see cref="None"/>. Reads two elements at most.
    /// </remarks>
    /// <typeparam name="T">The type of the elements.</typeparam>
    /// <param name="source">The elements.</param>
    /// <returns>The only element, or <see cref="None"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> is null.</exception>
    public static Maybe<T> SingleOrNone<T>(this IEnumerable<T> source)
        where T : notnull
    {
        ArgumentNullException.ThrowIfNull(source);
        using var values = source.GetEnumerator();
        if (!values.MoveNext())
        {
            return none;
        }

        var first = values.Current;
        return values.MoveNext() ? none : first;
    }

    /// <summary>
    /// The only element that satisfies <paramref name="predicate"/>, or <see cref="None"/> when not exactly one does.
    /// </summary>
    /// <remarks>Stops reading at the second match.</remarks>
    /// <typeparam name="T">The type of the elements.</typeparam>
    /// <param name="source">The elements.</param>
    /// <param name="predicate">The condition to satisfy.</param>
    /// <returns>The only matching element, or <see cref="None"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="predicate"/> is null.</exception>
    public static Maybe<T> SingleOrNone<T>(this IEnumerable<T> source, Func<T, bool> predicate)
        where T : notnull
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(predicate);
        Maybe<T> candidate = none;
        var found = false;
        foreach (var value in source)
        {
            if (!predicate(value))
            {
                continue;
            }

            if (found)
            {
                return none;
            }

            candidate = value;
            found = true;
        }

        return candidate;
    }

    // ---- dictionaries ----

    /// <summary>
    /// The value stored under <paramref name="key"/>, or <see cref="None"/> when there is none.
    /// </summary>
    /// <remarks>
    /// The maybe counterpart of <c>GetValueOrDefault</c>. A <see langword="null"/> stored value gives <see cref="None"/>.
    /// </remarks>
    /// <typeparam name="TKey">The type of the keys.</typeparam>
    /// <typeparam name="TValue">The type of the values.</typeparam>
    /// <param name="dictionary">The dictionary.</param>
    /// <param name="key">The key to look up.</param>
    /// <returns>The value, or <see cref="None"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="dictionary"/> is null.</exception>
    public static Maybe<TValue> GetValueOrNone<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> dictionary, TKey key)
        where TKey : notnull
        where TValue : notnull
    {
        ArgumentNullException.ThrowIfNull(dictionary);
        return dictionary.TryGetValue(key, out var value) ? value : none;
    }
}
