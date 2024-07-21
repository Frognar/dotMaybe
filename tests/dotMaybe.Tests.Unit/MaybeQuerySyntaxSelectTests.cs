namespace dotMaybe.Tests.Unit;

public class MaybeQuerySyntaxSelectTests
{
    [Property]
    public void Select_WhenSome_TransformsValue(int value)
    {
        (from x in Some.With(value)
                select x.ToString())
            .Should()
            .Be(Some.With(value.ToString()));
    }

    [Fact]
    public void Select_WhenNone_ReturnsNone()
    {
        (from x in None.OfType<int>()
                select x.ToString())
            .Should()
            .Be(None.OfType<string>());
    }
}
