namespace dotMaybe.Tests.Unit;

public class MaybeQuerySyntaxSelectManyTests
{
    [Property]
    public void SelectMany_WhenSome_TransformsValue(int value)
    {
        (from x in Some.With(value)
                from y in Some.With(value)
                select x + y)
            .Should()
            .Be(Some.With(value + value));
    }

    [Property]
    public void SelectMany_WhenFirstNone_ReturnsNone(int value)
    {
        (from x in None.OfType<int>()
                from y in Some.With(value)
                select x + y)
            .Should()
            .Be(None.OfType<int>());
    }

    [Property]
    public void SelectMany_WhenOtherNone_ReturnsNone(int value)
    {
        (from x in Some.With(value)
                from y in None.OfType<int>()
                select x + y)
            .Should()
            .Be(None.OfType<int>());
    }
}
