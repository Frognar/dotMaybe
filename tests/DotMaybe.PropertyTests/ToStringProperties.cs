using FsCheck.Xunit;

namespace DotMaybe.PropertyTests;

/// <summary>
/// The text form used in assertion messages, logs and the debugger: <c>Some(value)</c> or <c>None</c>.
/// </summary>
public sealed class ToStringProperties
{
    [Property]
    public void A_value_is_shown_as_Some_of_the_value(int value) =>
        Assert.Equal($"Some({value})", Maybes.Some(value).ToString());

    [Property]
    public void A_reference_value_is_shown_as_Some_of_the_value(string value) =>
        Assert.Equal($"Some({value})", Maybes.Some(value).ToString());

    [Fact]
    public void None_is_shown_as_None()
    {
        Assert.Equal("None", Maybes.Empty<int>().ToString());
        Assert.Equal("None", default(Maybe<string>).ToString());
    }

    [Fact]
    public void Nested_maybes_show_every_level()
    {
        Assert.Equal("Some(Some(42))", Maybes.Some(Maybes.Some(42)).ToString());
        Assert.Equal("Some(None)", Maybes.Some(Maybes.Empty<int>()).ToString());
    }
}
