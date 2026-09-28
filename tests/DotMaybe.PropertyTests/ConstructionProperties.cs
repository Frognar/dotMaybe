using FsCheck.Xunit;
using static DotMaybe.Prelude;

namespace DotMaybe.PropertyTests;

/// <summary>
/// Values get into a maybe only through the union conversions, and every maybe is exactly one of two cases.
/// </summary>
public sealed class ConstructionProperties
{
    [Property]
    public void A_value_converts_to_the_value_case(int value)
    {
        Maybe<int> maybe = value;

        int? held = maybe is int number ? number : null;
        Assert.Equal(value, held);
    }

    [Property]
    public void A_reference_value_converts_to_the_value_case(string value)
    {
        Maybe<string> maybe = value;

        var held = maybe is string text ? text : null;
        Assert.Equal(value, held);
    }

    [Property]
    public void A_value_converts_to_the_value_case_in_generic_code(int value) =>
        MaybeAssert.HoldsValue(value, Maybes.Some(value));

    [Fact]
    public void None_converts_to_the_empty_case()
    {
        Maybe<int> number = none;
        Maybe<string> text = none;

        MaybeAssert.IsNone(number);
        MaybeAssert.IsNone(text);
    }

    [Fact]
    public void Default_is_the_empty_case()
    {
        MaybeAssert.IsNone(default(Maybe<int>));
        MaybeAssert.IsNone(default(Maybe<string>));
        MaybeAssert.IsNone(default(Maybe<Maybe<int>>));
    }

    [Fact]
    public void A_null_that_slips_through_becomes_the_empty_case()
    {
        // Nullable analysis warns about this (see the compiler contracts); at run time it must still be None.
        string text = null!;
        Maybe<string> maybe = text;

        MaybeAssert.IsNone(maybe);
        MaybeAssert.IsNone(Maybes.Some<string>(null!));
    }

    [Property]
    public void Every_maybe_is_exactly_one_case(Maybe<int> maybe) =>
        Assert.True(
            (maybe is int) != (maybe is None),
            $"Expected exactly one case to match, but the maybe is {Maybes.Describe(maybe)}.");

    [Property]
    public void Every_reference_maybe_is_exactly_one_case(Maybe<string> maybe) =>
        Assert.True(
            (maybe is string) != (maybe is None),
            $"Expected exactly one case to match, but the maybe is {Maybes.Describe(maybe)}.");

    [Property]
    public void Switch_and_is_patterns_agree(Maybe<int> maybe)
    {
        var bySwitch = maybe switch
        {
            int value => $"value {value}",
            None => "none",
        };

        var byIs = maybe is int held ? $"value {held}" : "none";

        Assert.Equal(byIs, bySwitch);
    }

    [Property]
    public void A_maybe_nested_in_a_maybe_is_kept_as_a_value(Maybe<int> inner)
    {
        // Wrapping never flattens: Some(None) is a value, not None.
        var outer = Maybes.Some(inner);

        MaybeAssert.HoldsValue(inner, outer);
    }
}
