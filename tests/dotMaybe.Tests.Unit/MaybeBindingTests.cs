namespace dotMaybe.Tests.Unit;

public class MaybeBindingTests
{
    [Property]
    public void Bind_WhenSome_BindsValue(int value)
    {
        Some.With(value)
            .Bind(v => Some.With(v.ToString()))
            .Should()
            .Be(Some.With(value.ToString()));
    }

    [Fact]
    public void Bind_WhenNone_ReturnsNone()
    {
        None.OfType<int>()
            .Bind(v => Some.With(v.ToString()))
            .Should()
            .Be(None.OfType<string>());
    }
}
