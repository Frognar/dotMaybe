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
}
