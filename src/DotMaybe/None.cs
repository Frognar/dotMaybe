namespace DotMaybe;

/// <summary>
/// The unit type that marks the absence of a value in <see cref="Maybe{T}"/>.
/// </summary>
/// <remarks>
/// <see cref="None"/> is the second case of the <see cref="Maybe{T}"/> union, so it is what pattern
/// matching sees when a <see cref="Maybe{T}"/> holds no value:
/// <code>
/// var text = maybe switch
/// {
///     int value => value.ToString(),
///     None => "nothing",
/// };
/// </code>
/// To create an empty <see cref="Maybe{T}"/>, use <see cref="Prelude.none"/>.
/// </remarks>
public readonly record struct None;
