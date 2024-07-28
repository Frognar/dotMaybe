namespace dotMaybe.Tests.Unit;

public class MaybeBindingTests
{
    [Property]
    public void Bind_WhenSome_BindsValue(int value)
    {
        Some.With(value)
            .Bind(v => Some.With(v.ToString()))
            .Should()
            .Be(Some.With(value.ToString()));
    }

    [Fact]
    public void Bind_WhenNone_ReturnsNone()
    {
        None.OfType<int>()
            .Bind(v => Some.With(v.ToString()))
            .Should()
            .Be(None.OfType<string>());
    }

    [Property]
    public async Task BindAsync_WhenSome_TransformsValueAsync(int value)
    {
        (await Some.With(value)
                .BindAsync(async v => await Task.FromResult(Some.With(v.ToString()))))
            .Should()
            .Be(Some.With(value.ToString()));
    }

    [Fact]
    public async Task BindAsync_WhenNone_ReturnsNone()
    {
        (await None.OfType<int>()
                .BindAsync(async v => await Task.FromResult(Some.With(v.ToString()))))
            .Should()
            .Be(None.OfType<string>());
    }
}
