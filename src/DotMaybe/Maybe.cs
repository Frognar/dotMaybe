using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using static DotMaybe.Prelude;

namespace DotMaybe;

/// <summary>
/// An optional value: either a value of type <typeparamref name="T"/> or <see cref="None"/>.
/// </summary>
/// <typeparam name="T">The type of the value. Never <see langword="null"/>.</typeparam>
/// <remarks>
/// <para>
/// <see cref="Maybe{T}"/> is a C# union with exactly two cases, <typeparamref name="T"/> and <see cref="None"/>.
/// Both convert implicitly to <see cref="Maybe{T}"/>, and a <c>switch</c> that handles both cases is exhaustive:
/// </para>
/// <code>
/// Maybe&lt;string&gt; greeting = "Hello, Monad!";
/// Maybe&lt;int&gt; nothing = none;
///
/// var text = greeting switch
/// {
///     string value => value,
///     None => "nothing",
/// };
/// </code>
/// <para>
/// There is no invalid state: <c>default(Maybe&lt;T&gt;)</c> is <see cref="None"/>, and a <see langword="null"/>
/// reaching the conversion at run time becomes <see cref="None"/> as well.
/// </para>
/// <para>
/// Two maybes are equal when both are <see cref="None"/>, or when both hold values that are equal according to
/// <see cref="EqualityComparer{T}.Default"/>.
/// </para>
/// </remarks>
[Union]
public readonly struct Maybe<T> : Maybe<T>.IUnionMembers, IEquatable<Maybe<T>>
    where T : notnull
{
    private readonly T _value;
    private readonly bool _isSome;

    private Maybe(T value, bool isSome) => (_value, _isSome) = (value, isSome);

    /// <summary>
    /// The union members the C# compiler uses for union conversions and pattern matching.
    /// </summary>
    /// <remarks>
    /// <see cref="Maybe{T}"/> implements the instance members explicitly, so they stay out of its public API.
    /// Use pattern matching instead of calling them.
    /// </remarks>
    public interface IUnionMembers
    {
        /// <summary>
        /// Creates a <see cref="Maybe{T}"/> that holds <paramref name="value"/>.
        /// A <see langword="null"/> that slips through at run time produces <see cref="None"/>.
        /// </summary>
        /// <param name="value">The value to wrap.</param>
        /// <returns>The value case, or <see cref="None"/> for <see langword="null"/>.</returns>
        [SuppressMessage("ReSharper", "ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract")]
        static Maybe<T> Create(T value) => value is null ? default : new Maybe<T>(value, true);

        /// <summary>
        /// Creates an empty <see cref="Maybe{T}"/>.
        /// </summary>
        /// <param name="value">The <see cref="None"/> marker.</param>
        /// <returns>The <see cref="None"/> case.</returns>
        static Maybe<T> Create(None value) => default;

        /// <summary>
        /// Gets the current case: a <c>T</c> or <see cref="None"/>. Never <see langword="null"/>.
        /// </summary>
        object Value { get; }

        /// <summary>
        /// Gets the value when this is the <c>T</c> case.
        /// </summary>
        /// <param name="value">The value when the method returns <see langword="true"/>; otherwise the default.</param>
        /// <returns><see langword="true"/> for the <c>T</c> case; otherwise <see langword="false"/>.</returns>
        bool TryGetValue(out T value);

        /// <summary>
        /// Gets the marker when this is the <see cref="None"/> case.
        /// </summary>
        /// <param name="value">The <see cref="None"/> marker.</param>
        /// <returns><see langword="true"/> for the <see cref="None"/> case; otherwise <see langword="false"/>.</returns>
        bool TryGetValue(out None value);
    }

    object IUnionMembers.Value => _isSome ? _value : default(None);

    bool IUnionMembers.TryGetValue(out T value)
    {
        value = _value;
        return _isSome;
    }

    bool IUnionMembers.TryGetValue(out None value)
    {
        value = default;
        return !_isSome;
    }

    /// <summary>
    /// Reduces this maybe to a single result: <paramref name="some"/> runs on the value,
    /// <paramref name="none"/> runs when there is none. Exactly one of them runs, exactly once.
    /// </summary>
    /// <remarks>
    /// This is the way to read a value in generic code, where <c>maybe is T value</c> does not compile.
    /// </remarks>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="some">Produces the result from the value.</param>
    /// <param name="none">Produces the result when there is no value.</param>
    /// <returns>The result of the branch that ran.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="some"/> or <paramref name="none"/> is null.</exception>
    public TResult Match<TResult>(Func<T, TResult> some, Func<TResult> none)
    {
        ArgumentNullException.ThrowIfNull(some);
        ArgumentNullException.ThrowIfNull(none);
        return _isSome ? some(_value) : none();
    }

    /// <summary>
    /// Returns the value, or <paramref name="fallback"/> when there is none.
    /// </summary>
    /// <param name="fallback">The result when there is no value.</param>
    /// <returns>The value or the fallback. Never <see langword="null"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="fallback"/> is null.</exception>
    public T OrDefault(T fallback)
    {
        ArgumentNullException.ThrowIfNull(fallback);
        return _isSome ? _value : fallback;
    }

    /// <summary>
    /// Returns the value, or the result of <paramref name="fallback"/> when there is none.
    /// <paramref name="fallback"/> runs only when there is no value.
    /// </summary>
    /// <param name="fallback">Produces the result when there is no value.</param>
    /// <returns>The value or the produced fallback.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="fallback"/> is null.</exception>
    public T OrDefault(Func<T> fallback)
    {
        ArgumentNullException.ThrowIfNull(fallback);
        return _isSome ? _value : fallback();
    }

    /// <summary>
    /// Transforms the value with <paramref name="map"/>; <see cref="None"/> stays <see cref="None"/>.
    /// </summary>
    /// <remarks>
    /// <paramref name="map"/> runs only when there is a value. A <see langword="null"/> result becomes
    /// <see cref="None"/>.
    /// </remarks>
    /// <typeparam name="TResult">The type of the transformed value.</typeparam>
    /// <param name="map">Transforms the value.</param>
    /// <returns>The transformed value, or <see cref="None"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="map"/> is null.</exception>
    public Maybe<TResult> Map<TResult>(Func<T, TResult> map)
        where TResult : notnull
    {
        ArgumentNullException.ThrowIfNull(map);
        return _isSome ? map(_value) : none;
    }

    /// <summary>
    /// Continues with <paramref name="bind"/>, which may itself produce <see cref="None"/>;
    /// <see cref="None"/> stays <see cref="None"/>.
    /// </summary>
    /// <remarks>
    /// <paramref name="bind"/> runs only when there is a value. Use it to chain steps that can each fail.
    /// </remarks>
    /// <typeparam name="TResult">The type of the value produced by the next step.</typeparam>
    /// <param name="bind">The next step.</param>
    /// <returns>The result of the next step, or <see cref="None"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="bind"/> is null.</exception>
    public Maybe<TResult> Bind<TResult>(Func<T, Maybe<TResult>> bind)
        where TResult : notnull
    {
        ArgumentNullException.ThrowIfNull(bind);
        return _isSome ? bind(_value) : none;
    }

    /// <summary>
    /// Keeps the value only when it satisfies <paramref name="predicate"/>; otherwise <see cref="None"/>.
    /// </summary>
    /// <remarks>
    /// <paramref name="predicate"/> runs only when there is a value.
    /// </remarks>
    /// <param name="predicate">The condition the value must satisfy.</param>
    /// <returns>This maybe when the value satisfies <paramref name="predicate"/>; otherwise <see cref="None"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="predicate"/> is null.</exception>
    public Maybe<T> Filter(Func<T, bool> predicate)
    {
        ArgumentNullException.ThrowIfNull(predicate);
        return _isSome && predicate(_value) ? this : none;
    }

    /// <summary>
    /// Compares two maybes: both <see cref="None"/>, or both holding equal values.
    /// </summary>
    /// <param name="left">The first maybe.</param>
    /// <param name="right">The second maybe.</param>
    /// <returns><see langword="true"/> when the maybes are equal; otherwise <see langword="false"/>.</returns>
    public static bool operator ==(Maybe<T> left, Maybe<T> right) => left.Equals(right);

    /// <summary>
    /// Compares two maybes for inequality. Always the opposite of <c>==</c>.
    /// </summary>
    /// <param name="left">The first maybe.</param>
    /// <param name="right">The second maybe.</param>
    /// <returns><see langword="true"/> when the maybes differ; otherwise <see langword="false"/>.</returns>
    public static bool operator !=(Maybe<T> left, Maybe<T> right) => !left.Equals(right);

    /// <summary>
    /// Determines whether this maybe equals <paramref name="other"/>: both <see cref="None"/>, or both holding
    /// values that are equal according to <see cref="EqualityComparer{T}.Default"/>.
    /// </summary>
    /// <param name="other">The maybe to compare with.</param>
    /// <returns><see langword="true"/> when the maybes are equal; otherwise <see langword="false"/>.</returns>
    public bool Equals(Maybe<T> other) =>
        _isSome == other._isSome
        && (!_isSome || EqualityComparer<T>.Default.Equals(_value, other._value));

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is Maybe<T> maybe && Equals(maybe);

    /// <inheritdoc/>
    public override int GetHashCode() => _isSome ? HashCode.Combine(true, _value) : 0;

    /// <summary>
    /// Returns <c>Some(value)</c> for a value and <c>None</c> for the empty case.
    /// </summary>
    /// <returns>A text representation of this maybe.</returns>
    public override string ToString() => _isSome ? $"Some({_value})" : "None";
}
