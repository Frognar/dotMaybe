namespace DotMaybe.Json;

/// <summary>
/// Lets <see cref="MaybeJsonConverter{T}"/> see whether a value of type <c>T</c> is itself a maybe without a value,
/// to refuse writing <c>Some(None)</c>.
/// </summary>
/// <remarks>
/// Implemented explicitly, so it adds nothing to the public surface of <see cref="Maybe{T}"/>. The property is not
/// called <c>HasValue</c> on purpose: that name has a meaning for C# unions.
/// </remarks>
internal interface INestedMaybe
{
    bool IsSome { get; }
}
