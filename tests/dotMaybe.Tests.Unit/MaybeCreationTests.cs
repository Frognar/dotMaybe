namespace dotMaybe.Tests.Unit;

public class MaybeCreationTests
{
    [Property]
    public void Maybe_WithValue_ShouldBeSome(int value)
    {
        Some.With(value)
            .Should()
            .Be(Some.With(value));
    }

    [Fact]
    public void Maybe_WithoutValue_ShouldBeNone()
    {
        None.OfType<int>()
            .Should()
            .Be(None.OfType<int>());
    }

    [Property]
    public void Maybe_FromValue_ShouldBeSome(object value)
    {
        Maybe<object> maybe = value;

        maybe
            .Should()
            .Be(Some.With(value));
    }

    [Fact]
    public void Maybe_FromNull_ShouldBeNone()
    {
        Maybe<object> maybe = null;

        maybe
            .Should()
            .Be(None.OfType<object>());
    }

    [Property]
    public void ToMaybe_FromValueType_ShouldBeSome(int value)
    {
        ((int?)value)
            .ToMaybe()
            .Should()
            .Be(Some.With(value));
    }

    [Fact]
    public void ToMaybe_FromValueTypeNull_ShouldBeNone()
    {
        ((int?)null)
            .ToMaybe()
            .Should()
            .Be(None.OfType<int>());
    }

    [Property]
    public void ToMaybe_FromReferenceTypeValue_ShouldBeSome(string value)
    {
        value
            .ToMaybe()
            .Should()
            .Be(Some.With(value));
    }

    [Fact]
    public void ToMaybe_FromReferenceTypeNull_ShouldBeNone()
    {
        ((string?)null)
            .ToMaybe()
            .Should()
            .Be(None.OfType<string>());
    }
}
