using FsCheck;

namespace dotMaybe.Tests.Unit;

public class MaybeFilteringTests
{
    [Property]
    public void Filter_WhenSomeMatchingPredicate_ReturnsSome(NonNegativeInt value)
    {
        Some.With(value.Item)
            .Filter(x => x >= 0)
            .Should()
            .Be(Some.With(value.Item));
    }
    [Property]
    public void Filter_WhenSomeNotMatchingPredicate_ReturnsNone(NegativeInt value)
    {
        Some.With(value.Item)
            .Filter(x => x >= 0)
            .Should()
            .Be(None.OfType<int>());
    }

    [Fact]
    public void Filter_WhenNone_ReturnsNone()
    {
        None.OfType<int>()
            .Filter(x => x >= 0)
            .Should()
            .Be(None.OfType<int>());
    }
}
