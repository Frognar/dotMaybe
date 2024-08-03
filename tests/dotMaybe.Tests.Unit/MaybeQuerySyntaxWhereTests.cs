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
    [Property]
    public async Task Where_WhenSomeMatchingAsyncPredicate_ReturnsSome(NonNegativeInt value)
    {
        (await (from x in Some.With(value.Item)
                where Task.FromResult(x >= 0)
                select x))
            .Should()
            .Be(Some.With(value.Item));
    }

    [Property]
    public async Task Where_WhenSomeNotMatchingAsyncPredicate_ReturnsNone(NegativeInt value)
    {
        (await (from x in Some.With(value.Item)
                where Task.FromResult(x >= 0)
                select x))
            .Should()
            .Be(None.OfType<int>());
    }

    [Fact]
    public async Task Where_WhenNoneAsyncPredicate_ReturnsNone()
    {
        (await (from x in None.OfType<int>()
                where Task.FromResult(x >= 0)
                select x))
            .Should()
            .Be(None.OfType<int>());
    }

    [Property]
    public async Task Where_WhenTaskSomeMatchingPredicate_ReturnsSome(NonNegativeInt value)
    {
        (await (from x in Task.FromResult(Some.With(value.Item))
                where x >= 0
                select x))
            .Should()
            .Be(Some.With(value.Item));
    }

    [Property]
    public async Task Where_WhenTaskSomeNotMatchingPredicate_ReturnsNone(NegativeInt value)
    {
        (await (from x in Task.FromResult(Some.With(value.Item))
                where x >= 0
                select x))
            .Should()
            .Be(None.OfType<int>());
    }

    [Fact]
    public async Task Where_WhenTaskNone_ReturnsNone()
    {
        (await (from x in Task.FromResult(None.OfType<int>())
                where x >= 0
                select x))
            .Should()
            .Be(None.OfType<int>());
    }
    [Property]
    public async Task Where_WhenTaskSomeMatchingAsyncPredicate_ReturnsSome(NonNegativeInt value)
    {
        (await (from x in Task.FromResult(Some.With(value.Item))
                where Task.FromResult(x >= 0)
                select x))
            .Should()
            .Be(Some.With(value.Item));
    }

    [Property]
    public async Task Where_WhenTaskSomeNotMatchingAsyncPredicate_ReturnsNone(NegativeInt value)
    {
        (await (from x in Task.FromResult(Some.With(value.Item))
                where Task.FromResult(x >= 0)
                select x))
            .Should()
            .Be(None.OfType<int>());
    }

    [Fact]
    public async Task Where_WhenTaskNoneAsyncPredicate_ReturnsNone()
    {
        (await (from x in Task.FromResult(None.OfType<int>())
                where Task.FromResult(x >= 0)
                select x))
            .Should()
            .Be(None.OfType<int>());
    }
}
