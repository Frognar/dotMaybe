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
    [Property]
    public async Task Filter_WhenSomeMatchingAsyncPredicate_ReturnsSome(NonNegativeInt value)
    {
        (await Some.With(value.Item)
                .FilterAsync(async x => await Task.FromResult(x >= 0)))
            .Should()
            .Be(Some.With(value.Item));
    }
    [Property]
    public async Task Filter_WhenSomeNotMatchingAsyncPredicate_ReturnsNone(NegativeInt value)
    {
        (await Some.With(value.Item)
                .FilterAsync(async x => await Task.FromResult(x >= 0)))
            .Should()
            .Be(None.OfType<int>());
    }

    [Fact]
    public async Task Filter_WhenNoneAsyncPredicate_ReturnsNone()
    {
        (await None.OfType<int>()
                .FilterAsync(async x => await Task.FromResult(x >= 0)))
            .Should()
            .Be(None.OfType<int>());
    }
}
