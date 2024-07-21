namespace dotMaybe.Tests.Unit;

public class MaybePropertiesTesting
{
    [Property]
    public void IsSome_WhenSome_ReturnsTrue(int value)
    {
        Some.With(value)
            .IsSome
            .Should()
            .BeTrue();
    }

    [Fact]
    public void IsSome_WhenNone_ReturnsFalse()
    {
        None.OfType<int>()
            .IsSome
            .Should()
            .BeFalse();
    }

    [Property]
    public void IsNone_WhenSome_ReturnsFalse(int value)
    {
        Some.With(value)
            .IsNone
            .Should()
            .BeFalse();
    }

    [Fact]
    public void IsNone_WhenNone_ReturnsTrue()
    {
        None.OfType<int>()
            .IsNone
            .Should()
            .BeTrue();
    }
}
