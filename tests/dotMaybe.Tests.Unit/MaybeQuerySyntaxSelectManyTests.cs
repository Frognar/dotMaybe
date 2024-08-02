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

    [Property]
    public async Task SelectMany_WhenSomeAsyncResultSelector_TransformsValue(int value)
    {
        (await (from x in Some.With(value)
                from y in Some.With(value)
                select Task.FromResult(x + y)))
            .Should()
            .Be(Some.With(value + value));
    }

    [Property]
    public async Task SelectMany_WhenFirstAsyncResultSelector_TransformsValue(int value)
    {
        (await (from x in None.OfType<int>()
                from y in Some.With(value)
                select Task.FromResult(x + y)))
            .Should()
            .Be(None.OfType<int>());
    }

    [Property]
    public async Task SelectMany_WhenOtherAsyncResultSelector_TransformsValue(int value)
    {
        (await (from x in Some.With(value)
                from y in None.OfType<int>()
                select Task.FromResult(x + y)))
            .Should()
            .Be(None.OfType<int>());
    }

    [Property]
    public async Task SelectMany_WhenSomeAsyncIntermediateSelector_TransformsValue(int value)
    {
        (await (from x in Some.With(value)
                from y in Task.FromResult(Some.With(value))
                select x + y))
            .Should()
            .Be(Some.With(value + value));
    }

    [Property]
    public async Task SelectMany_WhenFirstAsyncIntermediateSelector_TransformsValue(int value)
    {
        (await (from x in None.OfType<int>()
                from y in Task.FromResult(Some.With(value))
                select x + y))
            .Should()
            .Be(None.OfType<int>());
    }

    [Property]
    public async Task SelectMany_WhenOtherAsyncIntermediateSelector_TransformsValue(int value)
    {
        (await (from x in Some.With(value)
                from y in Task.FromResult(None.OfType<int>())
                select x + y))
            .Should()
            .Be(None.OfType<int>());
    }

    [Property]
    public async Task SelectMany_WhenSomeAsyncIntermediateAndResultSelectors_TransformsValue(int value)
    {
        (await (from x in Some.With(value)
                from y in Task.FromResult(Some.With(value))
                select Task.FromResult(x + y)))
            .Should()
            .Be(Some.With(value + value));
    }

    [Property]
    public async Task SelectMany_WhenFirstAsyncIntermediateAndResultSelectors_TransformsValue(int value)
    {
        (await (from x in None.OfType<int>()
                from y in Task.FromResult(Some.With(value))
                select Task.FromResult(x + y)))
            .Should()
            .Be(None.OfType<int>());
    }

    [Property]
    public async Task SelectMany_WhenOtherAsyncIntermediateAndResultSelectors_TransformsValue(int value)
    {
        (await (from x in Some.With(value)
                from y in Task.FromResult(None.OfType<int>())
                select Task.FromResult(x + y)))
            .Should()
            .Be(None.OfType<int>());
    }

    [Property]
    public async Task SelectMany_WhenSomeFirstTask_TransformsValue(int value)
    {
        (await (from x in Task.FromResult(Some.With(value))
                from y in Some.With(value)
                select x + y))
            .Should()
            .Be(Some.With(value + value));
    }

    [Property]
    public async Task SelectMany_WhenFirstNoneFirstTask_ReturnsNone(int value)
    {
        (await (from x in Task.FromResult(None.OfType<int>())
                from y in Some.With(value)
                select x + y))
            .Should()
            .Be(None.OfType<int>());
    }

    [Property]
    public async Task SelectMany_WhenOtherNoneFirstTask_ReturnsNone(int value)
    {
        (await (from x in Task.FromResult(Some.With(value))
                from y in None.OfType<int>()
                select x + y))
            .Should()
            .Be(None.OfType<int>());
    }

    [Property]
    public async Task SelectMany_WhenSomeFirstTaskAsyncIntermediateSelector_TransformsValue(int value)
    {
        (await (from x in Task.FromResult(Some.With(value))
                from y in Task.FromResult(Some.With(value))
                select x + y))
            .Should()
            .Be(Some.With(value + value));
    }

    [Property]
    public async Task SelectMany_WhenFirstNoneFirstTaskAsyncIntermediateSelector_ReturnsNone(int value)
    {
        (await (from x in Task.FromResult(None.OfType<int>())
                from y in Task.FromResult(Some.With(value))
                select x + y))
            .Should()
            .Be(None.OfType<int>());
    }

    [Property]
    public async Task SelectMany_WhenOtherNoneFirstTaskAsyncIntermediateSelector_ReturnsNone(int value)
    {
        (await (from x in Task.FromResult(Some.With(value))
                from y in Task.FromResult(None.OfType<int>())
                select x + y))
            .Should()
            .Be(None.OfType<int>());
    }
}
