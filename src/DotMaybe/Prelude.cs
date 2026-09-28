namespace DotMaybe;

/// <summary>
/// Terse, keyword-like helpers for working with <see cref="Maybe{T}"/>.
/// </summary>
/// <remarks>
/// Import them explicitly with <c>using static DotMaybe.Prelude;</c>. The package never adds this import for you.
/// </remarks>
public static class Prelude
{
    /// <summary>
    /// The empty value. It converts implicitly to <see cref="Maybe{T}"/> of any <c>T</c>.
    /// </summary>
    /// <remarks>
    /// The name is lower case on purpose: the type <see cref="None"/> is used in pattern matching, and a value with
    /// the same name would make a bare <c>None</c> ambiguous wherever both <c>DotMaybe</c> and this class are imported.
    /// </remarks>
    /// <example>
    /// <code>
    /// Maybe&lt;int&gt; maybe = none;
    /// Maybe&lt;int&gt; parsed = ok ? 42 : none;
    /// </code>
    /// </example>
#pragma warning disable IDE1006 // Naming rule violation: lower-case name is intentional (keyword-like value).
    public static None none => throw new NotImplementedException();
#pragma warning restore IDE1006
}
