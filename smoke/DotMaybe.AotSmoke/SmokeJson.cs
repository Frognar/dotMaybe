using System.Text.Json.Serialization;

namespace DotMaybe.AotSmoke;

internal sealed record Order(Maybe<string> Note, Maybe<int> Quantity);

[JsonSerializable(typeof(int))]
[JsonSerializable(typeof(string))]
[JsonSerializable(typeof(Maybe<int>))]
[JsonSerializable(typeof(Maybe<string>))]
[JsonSerializable(typeof(Maybe<Maybe<int>>))]
[JsonSerializable(typeof(Order))]
internal sealed partial class SmokeJsonContext : JsonSerializerContext
{
}
