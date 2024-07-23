namespace dotMaybe.Tests.Unit;

public class MaybeFlatteningTests
{
    [Property]
    public void Flatten_FromMaybeOfMaybeOfT_ReturnsMaybeOfT(int value)
    {
        Some.With(Some.With(value))
            .Flatten()
            .Should()
            .Be(Some.With(value));
    }

    [Property]
    public void Flatten_FromDeeplyNestedMaybe_ReturnsMaybeOneLevelUp(int value)
    {
        Some.With(Some.With(Some.With(value)))
            .Flatten()
            .Should()
            .Be(Some.With(Some.With(value)));
    }
}
