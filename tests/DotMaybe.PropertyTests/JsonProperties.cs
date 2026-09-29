using System.Text.Json;
using System.Text.Json.Serialization;
using FsCheck.Xunit;
using static DotMaybe.Prelude;

namespace DotMaybe.PropertyTests;

/// <summary>
/// JSON with <c>System.Text.Json</c> and no configuration: a value is written as itself, <see cref="None"/> as
/// <c>null</c>, and reading gives back what was written. The only exception is a nested <c>Some(None)</c>, which cannot
/// be told apart from <see cref="None"/> in this format, so writing it fails instead of losing information.
/// Reflection-based and source-generated serialization agree.
/// </summary>
public sealed class JsonProperties
{
    // ---- format ----

    [Property]
    public void A_value_is_written_as_the_value_itself(int number, string text)
    {
        Assert.Equal(JsonSerializer.Serialize(number), JsonSerializer.Serialize(Maybes.Some(number)));
        Assert.Equal(JsonSerializer.Serialize(text), JsonSerializer.Serialize(Maybes.Some(text)));
    }

    [Fact]
    public void None_is_written_as_null()
    {
        Assert.Equal("null", JsonSerializer.Serialize(Maybes.Empty<int>()));
        Assert.Equal("null", JsonSerializer.Serialize(Maybes.Empty<string>()));
        Assert.Equal("null", JsonSerializer.Serialize(default(Maybe<Person>)));
    }

    [Fact]
    public void Arrays_hold_values_and_nulls()
    {
        Maybe<int>[] maybes = [1, none, 3];

        Assert.Equal("[1,null,3]", JsonSerializer.Serialize(maybes));
        Assert.Equal(maybes, JsonSerializer.Deserialize<Maybe<int>[]>("[1,null,3]"));
    }

    // ---- round trips ----

    [Property]
    public void Maybes_round_trip(Maybe<int> number, Maybe<string> text, Maybe<Guid> id)
    {
        Assert.Equal(number, RoundTrip(number));
        Assert.Equal(text, RoundTrip(text));
        Assert.Equal(id, RoundTrip(id));
    }

    [Property]
    public void Object_values_round_trip(Maybe<int> presence, string name, int age)
    {
        var person = presence.Map(_ => new Person(name, age));

        Assert.Equal(person, RoundTrip(person));
    }

    [Property]
    public void Arrays_of_maybes_round_trip(Maybe<int>[] maybes) =>
        Assert.Equal(maybes, RoundTrip(maybes));

    [Property]
    public void Documents_with_optional_properties_round_trip(string name, Maybe<string> nickname, Maybe<int> age)
    {
        var profile = new Profile(name, nickname, age);

        Assert.Equal(profile, RoundTrip(profile));
    }

    // ---- documents ----

    [Fact]
    public void Missing_and_null_properties_are_None()
    {
        var expected = new Profile("Ada", none, none);

        Assert.Equal(expected, JsonSerializer.Deserialize<Profile>("""{"Name":"Ada"}"""));
        Assert.Equal(expected, JsonSerializer.Deserialize<Profile>("""{"Name":"Ada","Nickname":null,"Age":null}"""));
    }

    [Fact]
    public void None_properties_are_written_as_null_or_left_out_with_WhenWritingDefault()
    {
        var profile = new Profile("Ada", none, 36);
        var leaveOut = new JsonSerializerOptions { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault };

        Assert.Equal("""{"Name":"Ada","Nickname":null,"Age":36}""", JsonSerializer.Serialize(profile));
        Assert.Equal("""{"Name":"Ada","Age":36}""", JsonSerializer.Serialize(profile, leaveOut));
    }

    [Fact]
    public void The_options_of_the_call_apply_to_the_value()
    {
        var enumsAsText = new JsonSerializerOptions { Converters = { new JsonStringEnumConverter() } };

        Assert.Equal(
            """{"name":"Ada","age":36}""",
            JsonSerializer.Serialize(Maybes.Some(new Person("Ada", 36)), JsonSerializerOptions.Web));
        Assert.Equal(
            Maybes.Some(new Person("Ada", 36)),
            JsonSerializer.Deserialize<Maybe<Person>>("""{"name":"Ada","age":36}""", JsonSerializerOptions.Web));
        Assert.Equal("\"Monday\"", JsonSerializer.Serialize(Maybes.Some(DayOfWeek.Monday), enumsAsText));
        Assert.Equal(
            Maybes.Some(DayOfWeek.Monday),
            JsonSerializer.Deserialize<Maybe<DayOfWeek>>("\"Monday\"", enumsAsText));
    }

    [Fact]
    public void A_value_that_is_not_valid_for_the_type_is_rejected()
    {
        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<Maybe<int>>("\"five\""));
        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<Maybe<int>>("{}"));
    }

    // ---- nested maybes ----

    [Fact]
    public void Nested_maybes_use_one_null_for_None()
    {
        Assert.Equal("5", JsonSerializer.Serialize(Maybes.Some(Maybes.Some(5))));
        Assert.Equal("null", JsonSerializer.Serialize(Maybes.Empty<Maybe<int>>()));
        Assert.Equal(Maybes.Some(Maybes.Some(5)), JsonSerializer.Deserialize<Maybe<Maybe<int>>>("5"));
        Assert.Equal(Maybes.Empty<Maybe<int>>(), JsonSerializer.Deserialize<Maybe<Maybe<int>>>("null"));
    }

    [Property]
    public void Nested_maybes_round_trip_or_refuse_to_lose_information(Maybe<Maybe<int>> nested)
    {
        if (IsValueHoldingNone(nested))
        {
            Assert.Throws<JsonException>(() => JsonSerializer.Serialize(nested));
            return;
        }

        Assert.Equal(nested, RoundTrip(nested));
    }

    // ---- source generation (the trimming and Native AOT path) ----

    [Property]
    public void Source_generated_serialization_writes_the_same_json(
        Maybe<int> number,
        Maybe<string> text,
        string name,
        Maybe<int> age)
    {
        var profile = new Profile(name, text, age);

        Assert.Equal(JsonSerializer.Serialize(number), WriteGenerated(number));
        Assert.Equal(JsonSerializer.Serialize(text), WriteGenerated(text));
        Assert.Equal(JsonSerializer.Serialize(profile), WriteGenerated(profile));
    }

    [Property]
    public void Source_generated_serialization_round_trips(
        Maybe<int> number,
        Maybe<int>[] numbers,
        Maybe<int> presence,
        string name,
        int age)
    {
        var person = presence.Map(_ => new Person(name, age));
        var profile = new Profile(name, none, number);

        Assert.Equal(number, ReadGenerated<Maybe<int>>(WriteGenerated(number)));
        Assert.Equal(numbers, ReadGenerated<Maybe<int>[]>(WriteGenerated(numbers)));
        Assert.Equal(person, ReadGenerated<Maybe<Person>>(WriteGenerated(person)));
        Assert.Equal(profile, ReadGenerated<Profile>(WriteGenerated(profile)));
    }

    [Fact]
    public void Source_generated_nested_maybes_behave_the_same()
    {
        Assert.Equal("5", WriteGenerated(Maybes.Some(Maybes.Some(5))));
        Assert.Equal(Maybes.Empty<Maybe<int>>(), ReadGenerated<Maybe<Maybe<int>>>("null"));
        Assert.Throws<JsonException>(() => WriteGenerated(Maybes.Some(Maybes.Empty<int>())));
    }

    private static T RoundTrip<T>(T value) =>
        JsonSerializer.Deserialize<T>(JsonSerializer.Serialize(value))!;

    private static string WriteGenerated<T>(T value) =>
        JsonSerializer.Serialize(value, typeof(T), MaybeJsonContext.Default);

    private static T ReadGenerated<T>(string json) =>
        (T)JsonSerializer.Deserialize(json, typeof(T), MaybeJsonContext.Default)!;

    private static bool IsValueHoldingNone(Maybe<Maybe<int>> nested) =>
        nested.Match(inner => inner.Match(_ => false, () => true), () => false);
}
