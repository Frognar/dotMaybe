using System.Runtime.CompilerServices;

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
/// </remarks>
[Union]
public readonly struct Maybe<T> : Maybe<T>.IUnionMembers
    where T : notnull
{
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
        static Maybe<T> Create(T value) => throw new NotImplementedException();

        /// <summary>
        /// Creates an empty <see cref="Maybe{T}"/>.
        /// </summary>
        /// <param name="value">The <see cref="None"/> marker.</param>
        /// <returns>The <see cref="None"/> case.</returns>
        static Maybe<T> Create(None value) => throw new NotImplementedException();

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

    object IUnionMembers.Value => throw new NotImplementedException();

    bool IUnionMembers.TryGetValue(out T value) => throw new NotImplementedException();

    bool IUnionMembers.TryGetValue(out None value) => throw new NotImplementedException();
}
