namespace DotMaybe;

/// <content>
/// Ordering: <see cref="None"/> comes first, then the values in their own order.
/// </content>
/// <remarks>
/// There are deliberately no <c>&lt;</c>, <c>&gt;</c>, <c>&lt;=</c> or <c>&gt;=</c> operators. With the implicit
/// conversion from values they would make <c>maybeAge &lt; 18</c> compile and be <see langword="true"/> for
/// <see cref="None"/>, the opposite of what lifted operators on <see cref="Nullable{T}"/> do.
/// Sorting, <c>Order()</c>, <c>Min()</c>, <c>Max()</c> and <see cref="Comparer{T}.Default"/> use
/// <see cref="CompareTo(Maybe{T})"/>.
/// </remarks>
public readonly partial struct Maybe<T> : IComparable<Maybe<T>>, IComparable
{
    /// <summary>
    /// Compares with <paramref name="other"/>: <see cref="None"/> comes before every value, two
    /// <see cref="None"/>s are equal, and two values compare with <see cref="Comparer{T}.Default"/>.
    /// </summary>
    /// <param name="other">The maybe to compare with.</param>
    /// <returns>
    /// Less than zero when this maybe comes first, zero when both are in the same position, greater than zero when
    /// <paramref name="other"/> comes first.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Both have a value and <typeparamref name="T"/> cannot be compared (it implements neither
    /// <see cref="IComparable{T}"/> nor <see cref="IComparable"/>), as with <see cref="ValueTuple{T1}"/>.
    /// </exception>
    public int CompareTo(Maybe<T> other)
    {
        return (_isSome, other._isSome) switch
        {
            (true, true) => Comparer<T>.Default.Compare(_value, other._value),
            (true, false) => 1,
            (false, true) => -1,
            (false, false) => 0,
        };
    }

    /// <summary>
    /// Compares with a boxed <see cref="Maybe{T}"/>; every maybe comes after <see langword="null"/>.
    /// </summary>
    /// <param name="obj">A boxed <see cref="Maybe{T}"/>, or <see langword="null"/>.</param>
    /// <returns>The same as <see cref="CompareTo(Maybe{T})"/>; greater than zero for <see langword="null"/>.</returns>
    /// <exception cref="ArgumentException"><paramref name="obj"/> is not a <see cref="Maybe{T}"/>.</exception>
    int IComparable.CompareTo(object? obj)
    {
        if (obj is null)
        {
            return 1;
        }

        return obj is Maybe<T> other
            ? CompareTo(other)
            : throw new ArgumentException("obj is not a Maybe<T>.", nameof(obj));
    }
}
