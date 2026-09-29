using System.Text.Json.Serialization;
using DotMaybe.Json;

namespace DotMaybe;

/// <content>
/// JSON: the value itself, or <c>null</c> for <see cref="None"/> (see <see cref="MaybeJsonConverter{T}"/>).
/// </content>
[JsonConverter(typeof(MaybeJsonConverter<>))]
public readonly partial struct Maybe<T> : INestedMaybe
{
    bool INestedMaybe.IsSome => _isSome;
}
