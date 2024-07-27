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
    public void FirstOrNone_NonEmptyCollection_ReturnsFirstElement(PositiveInt value)
    {
        Enumerable.Range(value.Item, 10)
            .FirstOrNone()
            .Should()
            .Be(Some.With(value.Item));
    }

    [Property]
    public void FirstOrNone_CollectionWithMatchingElements_ReturnsFirstMatch(PositiveInt value)
    {
        var list = Enumerable.Range(value.Item, 10).ToList();
        list
            .FirstOrNone(v => v > value.Item + 2)
            .Should()
            .Be(Some.With(list[3]));
    }

    [Property]
    public void FirstOrNone_CollectionWithNoMatchingElements_ReturnsNone(PositiveInt value)
    {
        Enumerable.Range(value.Item, 10)
            .FirstOrNone(v => v > value.Item + 15)
            .Should()
            .Be(None.OfType<int>());
    }
}
