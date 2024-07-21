using FsCheck;

namespace dotMaybe.Tests.Unit;

public class MaybeQuerySyntaxWhereTests
{
    [Property]
    public void Where_WhenSomeMatchingPredicate_ReturnsSome(NonNegativeInt value)
    {
        (from x in Some.With(value.Item)
                where x >= 0
                select x)
            .Should()
            .Be(Some.With(value.Item));
    }

    [Property]
    public void Where_WhenSomeNotMatchingPredicate_ReturnsNone(NegativeInt value)
    {
        (from x in Some.With(value.Item)
                where x >= 0
                select x)
            .Should()
            .Be(None.OfType<int>());
    }

    [Fact]
    public void Where_WhenNone_ReturnsNone()
    {
        (from x in None.OfType<int>()
                where x >= 0
                select x)
            .Should()
            .Be(None.OfType<int>());
    }
}
