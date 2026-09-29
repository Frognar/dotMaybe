using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Globalization;
using FsCheck.Xunit;

namespace DotMaybe.PropertyTests;

/// <summary>
/// Maybes meet the rest of .NET: dictionary lookups, nullable values and references, and parsing.
/// Every conversion agrees with the .NET API it wraps, and <see langword="null"/> always means <see cref="None"/>.
/// </summary>
public sealed class InteropProperties
{
    // ---- GetValueOrNone ----

    [Property]
    public void GetValueOrNone_agrees_with_TryGetValue(int[] keys, int key)
    {
        var dictionary = keys.Distinct().ToDictionary(stored => stored, stored => $"value {stored}");
        var expected = dictionary.TryGetValue(key, out var value) ? Maybes.Some(value) : Maybes.Empty<string>();

        Assert.Equal(expected, dictionary.GetValueOrNone(key));
    }

    [Fact]
    public void GetValueOrNone_works_on_any_read_only_dictionary()
    {
        var dictionary = new Dictionary<int, string> { [1] = "one" };
        IReadOnlyDictionary<int, string>[] dictionaries =
        [
            dictionary,
            new SortedDictionary<int, string>(dictionary),
            new ConcurrentDictionary<int, string>(dictionary),
            new ReadOnlyDictionary<int, string>(dictionary),
            dictionary.ToImmutableDictionary(),
        ];

        foreach (var readOnly in dictionaries)
        {
            Assert.Equal(Maybes.Some("one"), readOnly.GetValueOrNone(1));
            MaybeAssert.IsNone(readOnly.GetValueOrNone(2));
        }
    }

    [Fact]
    public void A_null_stored_value_gives_None()
    {
        var dictionary = new Dictionary<string, string> { ["key"] = null! };

        MaybeAssert.IsNone(dictionary.GetValueOrNone("key"));
    }

    [Fact]
    public void A_missing_dictionary_is_rejected()
    {
        IReadOnlyDictionary<int, string> dictionary = null!;

        var error = Assert.Throws<ArgumentNullException>(() => dictionary.GetValueOrNone(1));

        Assert.Equal("dictionary", error.ParamName);
    }

    // ---- FromNullable / ToNullable ----

    [Property]
    public void FromNullable_of_a_value_type_agrees_with_HasValue(int? value)
    {
        var expected = value.HasValue ? Maybes.Some(value.Value) : Maybes.Empty<int>();

        Assert.Equal(expected, Maybe.FromNullable(value));
    }

    [Property]
    public void FromNullable_of_a_reference_turns_null_into_None(string text, bool isNull)
    {
        string? value = isNull ? null : text;
        var expected = value is null ? Maybes.Empty<string>() : Maybes.Some(value);

        Assert.Equal(expected, Maybe.FromNullable(value));
    }

    [Property]
    public void ToNullable_of_a_value_type_is_Match(Maybe<int> maybe) =>
        Assert.Equal(maybe.Match<int?>(value => value, () => null), maybe.ToNullable());

    [Property]
    public void ToNullable_of_a_reference_is_Match(Maybe<string> maybe) =>
        Assert.Equal(maybe.Match<string?>(value => value, () => null), maybe.ToNullable());

    [Fact]
    public void None_becomes_null()
    {
        Assert.Null(Maybes.Empty<int>().ToNullable());
        Assert.Null(Maybes.Empty<string>().ToNullable());
        Assert.Null(default(Maybe<Guid>).ToNullable());
    }

    [Property]
    public void Maybes_round_trip_through_nullables(Maybe<int> number, Maybe<string> text)
    {
        Assert.Equal(number, Maybe.FromNullable(number.ToNullable()));
        Assert.Equal(text, Maybe.FromNullable(text.ToNullable()));
    }

    [Property]
    public void Nullables_round_trip_through_maybes(int? value) =>
        Assert.Equal(value, Maybe.FromNullable(value).ToNullable());

    // ---- Parse ----

    [Property]
    public void A_formatted_number_parses_back(int number) =>
        Assert.Equal(
            Maybes.Some(number),
            Maybe.Parse<int>(number.ToString(CultureInfo.InvariantCulture), CultureInfo.InvariantCulture));

    [Property]
    public void Parse_agrees_with_TryParse(string text)
    {
        var expected = int.TryParse(text, CultureInfo.InvariantCulture, out var number)
            ? Maybes.Some(number)
            : Maybes.Empty<int>();

        Assert.Equal(expected, Maybe.Parse<int>(text, CultureInfo.InvariantCulture));
    }

    [Property]
    public void Parse_without_a_provider_agrees_with_TryParse_without_one(string text)
    {
        var expected = int.TryParse(text, (IFormatProvider?)null, out var number)
            ? Maybes.Some(number)
            : Maybes.Empty<int>();

        Assert.Equal(expected, Maybe.Parse<int>(text));
    }

    [Property]
    public void Parse_works_for_any_parsable_type(Guid guid, decimal amount)
    {
        Assert.Equal(Maybes.Some(guid), Maybe.Parse<Guid>(guid.ToString()));
        Assert.Equal(
            Maybes.Some(amount),
            Maybe.Parse<decimal>(amount.ToString(CultureInfo.InvariantCulture), CultureInfo.InvariantCulture));
    }

    [Fact]
    public void Invalid_or_missing_text_gives_None()
    {
        MaybeAssert.IsNone(Maybe.Parse<int>("ten"));
        MaybeAssert.IsNone(Maybe.Parse<int>(""));
        MaybeAssert.IsNone(Maybe.Parse<int>(null));
        MaybeAssert.IsNone(Maybe.Parse<Guid>("not a guid", CultureInfo.InvariantCulture));
    }

    [Fact]
    public void The_provider_is_used()
    {
        var comma = new NumberFormatInfo { NumberDecimalSeparator = ",", NumberGroupSeparator = "." };

        Assert.Equal(Maybes.Some(1.5m), Maybe.Parse<decimal>("1,5", comma));
        Assert.Equal(Maybes.Some(1.5m), Maybe.Parse<decimal>("1.5", CultureInfo.InvariantCulture));
    }
}
