namespace dotMaybe.Tests.Unit;

public class MaybeOrDefaultTests
{
    [Property]
    public void OrDefault_WhenSome_ReturnsInternalValue(int value)
    {
        Some.With(value)
            .OrDefault(-1)
            .Should()
            .Be(value);
    }

    [Property]
    public void OrDefault_WhenNone_ReturnsDefault(int value)
    {
        None.OfType<int>()
            .OrDefault(value)
            .Should()
            .Be(value);
    }

    [Property]
    public void OrDefault_WhenSome_ReturnsInternalValueWithoutCallingFactory(int value)
    {
        Some.With(value)
            .OrDefault(Factory)
            .Should()
            .Be(value);

        int Factory() => throw new Exception();
    }

    [Property]
    public void OrDefault_WhenNone_ReturnsValueFromFactory(int value)
    {
        None.OfType<int>()
            .OrDefault(() => value)
            .Should()
            .Be(value);
    }
}
