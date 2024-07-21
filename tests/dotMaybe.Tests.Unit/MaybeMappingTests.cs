namespace dotMaybe.Tests.Unit;

public class MaybeMappingTests
{
    [Property]
    public void Map_WhenSome_TransformsValue(int value)
    {
        Some.With(value)
            .Map(v => v.ToString())
            .Should()
            .Be(Some.With(value.ToString()));
    }

    [Property]
    public void Map_WhenNone_ReturnsNone()
    {
        None.OfType<int>()
            .Map(v => v.ToString())
            .Should()
            .Be(None.OfType<string>());
    }
}
