using FsCheck;

namespace dotMaybe.Tests.Unit;

public class MaybeCollectionTests
{
    [Fact]
    public void FirstOrNone_EmptyCollection_ReturnsNone()
    {
        Enumerable.Empty<int>()
            .FirstOrNone()
            .Should()
            .Be(None.OfType<int>());
    }

    [Property]
    public void FirstOrNone_NonEmptyCollection_ReturnsSomeWithFirstElement(PositiveInt value)
    {
        Enumerable.Range(value.Item, value.Item + 10)
            .FirstOrNone()
            .Should()
            .Be(Some.With(value.Item));
    }
}
