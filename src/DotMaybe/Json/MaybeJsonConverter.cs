using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using static DotMaybe.Prelude;

namespace DotMaybe.Json;

/// <summary>
/// Reads and writes <see cref="Maybe{T}"/> as the value itself or <c>null</c>, like <see cref="Nullable{T}"/>.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="Maybe{T}"/> carries this converter in its <see cref="JsonConverterAttribute"/>, so no configuration is
/// needed: <c>Some(42)</c> is written as <c>42</c> and <see cref="None"/> as <c>null</c>. Reading <c>null</c> or a
/// missing property gives <see cref="None"/>. The value is read and written with the options of the call, so naming
/// policies and converters for <typeparamref name="T"/> apply.
/// </para>
/// <para>
/// To leave out properties without a value, use <see cref="JsonIgnoreCondition.WhenWritingDefault"/>:
/// <c>default(Maybe&lt;T&gt;)</c> is <see cref="None"/>.
/// </para>
/// <para>
/// A nested <c>Some(None)</c> would be written as <c>null</c> and read back as <see cref="None"/>, so writing it
/// throws <see cref="JsonException"/> instead of losing information.
/// </para>
/// <para>
/// The converter is compatible with trimming, Native AOT and source-generated contexts: the value goes through the
/// <see cref="System.Text.Json.Serialization.Metadata.JsonTypeInfo{T}"/> of the options. It can also be registered
/// for a closed type by hand, e.g. in <see cref="JsonSourceGenerationOptionsAttribute.Converters"/>.
/// </para>
/// </remarks>
/// <typeparam name="T">The type of the value.</typeparam>
public sealed class MaybeJsonConverter<T> : JsonConverter<Maybe<T>>
    where T : notnull
{
    // True only when T is itself a Maybe<U>; checked once per T, so other values are never boxed.
    private static readonly bool ValueIsMaybe = typeof(INestedMaybe).IsAssignableFrom(typeof(T));

    /// <summary>
    /// Always <see langword="true"/>: <c>null</c> is a valid input and means <see cref="None"/>.
    /// </summary>
    public override bool HandleNull => true;

    /// <inheritdoc/>
    public override Maybe<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
        {
            return none;
        }

        var value = JsonSerializer.Deserialize(ref reader, ValueInfo(options));
        return value is null ? none : value;
    }

    /// <inheritdoc/>
    /// <exception cref="JsonException"><paramref name="value"/> is a value holding a nested <see cref="None"/>.</exception>
    public override void Write(Utf8JsonWriter writer, Maybe<T> value, JsonSerializerOptions options)
    {
        ArgumentNullException.ThrowIfNull(writer);

        if (!value.TryGetSome(out var inner))
        {
            writer.WriteNullValue();
            return;
        }

        if (ValueIsMaybe && !((INestedMaybe)inner).IsSome)
        {
            throw new JsonException(
                "A value holding None (Some(None)) cannot be written: as null it would be read back as None.");
        }

        JsonSerializer.Serialize(writer, inner, ValueInfo(options));
    }

    // Through JsonTypeInfo<T> rather than reflection: works with source-generated contexts, trimming and Native AOT.
    private static JsonTypeInfo<T> ValueInfo(JsonSerializerOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        return options.GetTypeInfo<T>();
    }
}
