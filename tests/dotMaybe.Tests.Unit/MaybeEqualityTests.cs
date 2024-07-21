namespace dotMaybe.Tests.Unit;

public class MaybeEqualityTests
{
    [Property]
    public void Maybe_Some_ShouldNotBeEqualToNone(int value)
    {
        Some.With(value)
            .Should()
            .NotBe(None.OfType<int>());
    }
}
