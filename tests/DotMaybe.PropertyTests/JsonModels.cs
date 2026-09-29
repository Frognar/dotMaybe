using System.Text.Json.Serialization;

namespace DotMaybe.PropertyTests;

/// <summary>A document with optional properties, as an API would receive or return it.</summary>
public sealed record Profile(string Name, Maybe<string> Nickname, Maybe<int> Age);

/// <summary>An object value inside a maybe.</summary>
public sealed record Person(string Name, int Age);

/// <summary>
/// A source-generated context: the path used by trimmed and Native AOT applications. If the generator could not
/// handle the converter declared on <c>Maybe&lt;T&gt;</c>, it would report it at build time.
/// </summary>
/// <remarks>
/// The converter reads and writes the value through the context, so the value types (<c>int</c>, <c>string</c>,
/// <c>Person</c>) are listed too.
/// </remarks>
[JsonSerializable(typeof(int))]
[JsonSerializable(typeof(string))]
[JsonSerializable(typeof(Person))]
[JsonSerializable(typeof(Maybe<int>))]
[JsonSerializable(typeof(Maybe<string>))]
[JsonSerializable(typeof(Maybe<Person>))]
[JsonSerializable(typeof(Maybe<Maybe<int>>))]
[JsonSerializable(typeof(Maybe<int>[]))]
[JsonSerializable(typeof(Profile))]
internal sealed partial class MaybeJsonContext : JsonSerializerContext
{
}
