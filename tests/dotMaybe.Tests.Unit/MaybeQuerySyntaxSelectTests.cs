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

    [Property]
    public async Task Select_WhenSomeAsyncMap_TransformsValue(int value)
    {
        (await (from x in Some.With(value)
                select Task.FromResult(x.ToString())))
            .Should()
            .Be(Some.With(value.ToString()));
    }

    [Fact]
    public async Task Select_WhenNoneAsyncMap_ReturnsNone()
    {
        (await (from x in None.OfType<int>()
                select Task.FromResult(x.ToString())))
            .Should()
            .Be(None.OfType<string>());
    }

    [Property]
    public async Task Select_WhenTaskSome_TransformsValue(int value)
    {
        (await (from x in Task.FromResult(Some.With(value))
                select x.ToString()))
            .Should()
            .Be(Some.With(value.ToString()));
    }

    [Fact]
    public async Task Select_WhenTaskNone_ReturnsNone()
    {
        (await (from x in Task.FromResult(None.OfType<int>())
                select x.ToString()))
            .Should()
            .Be(None.OfType<string>());
    }

    [Property]
    public async Task Select_WhenTaskSomeAsyncMap_TransformsValue(int value)
    {
        (await (from x in Task.FromResult(Some.With(value))
                select Task.FromResult(x.ToString())))
            .Should()
            .Be(Some.With(value.ToString()));
    }

    [Fact]
    public async Task Select_WhenTaskNoneAsyncMap_ReturnsNone()
    {
        (await (from x in Task.FromResult(None.OfType<int>())
                select Task.FromResult(x.ToString())))
            .Should()
            .Be(None.OfType<string>());
    }
}
