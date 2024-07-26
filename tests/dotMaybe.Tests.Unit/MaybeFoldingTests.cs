namespace dotMaybe.Tests.Unit;

public class MaybeFoldingTests
{
    [Property]
    public void Fold_WhenSome_ReturnsStateUpdatedByGivenFunction(int state, int value)
    {
        Some.With(value)
            .Fold(state, (s, v) => s + v)
            .Should()
            .Be(state + value);
    }

    [Property]
    public void Fold_WhenNone_ReturnsStateUnchanged(int state)
    {
        None.OfType<int>()
            .Fold(state, (s, v) => s + v)
            .Should()
            .Be(state);
    }
}
