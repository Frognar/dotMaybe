namespace dotMaybe.Tests.Unit;

public class MaybeCreationTests
{
    [Property]
    public void Maybe_WithValue_ShouldBeSome(int value)
    {
        Some.With(value)
            .Should()
            .Be(Some.With(value));
    }

    [Fact]
    public void Maybe_WithoutValue_ShouldBeNone()
    {
        None.OfType<int>()
            .Should()
            .Be(None.OfType<int>());
    }
}
