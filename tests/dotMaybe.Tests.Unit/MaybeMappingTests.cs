namespace dotMaybe.Tests.Unit;

public class MaybeMappingTests
{
    [Property]
    public void Map_WhenSome_TransformsValue(int value)
    {
        Some.With(value)
            .Map(v => v.ToString())
            .Should()
            .Be(Some.With(value.ToString()));
    }

    [Property]
    public void Map_WhenNone_ReturnsNone()
    {
        None.OfType<int>()
            .Map(v => v.ToString())
            .Should()
            .Be(None.OfType<string>());
    }

    [Property]
    public async Task MapAsync_WhenSome_TransformsValueAsync(int value)
    {
        (await Some.With(value)
                .MapAsync(async v => await Task.FromResult(v.ToString())))
            .Should()
            .Be(Some.With(value.ToString()));
    }

    [Fact]
    public async Task MapAsync_WhenNone_ReturnsNone()
    {
        (await None.OfType<int>()
                .MapAsync(async v => await Task.FromResult(v.ToString())))
            .Should()
            .Be(None.OfType<string>());
    }

    [Property]
    public async Task MapAsync_WhenTaskSome_TransformsValueAsync(int value)
    {
        (await Task.FromResult(Some.With(value))
                .MapAsync(v =>v.ToString()))
            .Should()
            .Be(Some.With(value.ToString()));
    }

    [Fact]
    public async Task MapAsync_WhenTaskNone_ReturnsNone()
    {
        (await Task.FromResult(None.OfType<int>())
                .MapAsync(v => v.ToString()))
            .Should()
            .Be(None.OfType<string>());
    }

    [Property]
    public async Task MapAsync_WhenTaskSomeAsyncMap_WhenTransformsValueAsync(int value)
    {
        (await Task.FromResult(Some.With(value))
                .MapAsync(async v => await Task.FromResult(v.ToString())))
            .Should()
            .Be(Some.With(value.ToString()));
    }

    [Fact]
    public async Task MapAsync_WhenTaskNoneAsyncMap_WhenReturnsNone()
    {
        (await Task.FromResult(None.OfType<int>())
                .MapAsync(async v => await Task.FromResult(v.ToString())))
            .Should()
            .Be(None.OfType<string>());
    }

    [Property]
    public void Map2_WhenBothSome_ReturnsSome(int value, decimal otherValue)
    {
        Maybe.Map2(
                Some.With(value),
                Some.With(otherValue),
                (v1, v2) => $"{v1} : {v2}")
            .Should()
            .Be(Some.With($"{value} : {otherValue}"));
    }

    [Property]
    public void Map2_WhenFirstNone_ReturnsNone(decimal otherValue)
    {
        Maybe.Map2(
                None.OfType<int>(),
                Some.With(otherValue),
                (v1, v2) => $"{v1} : {v2}")
            .Should()
            .Be(None.OfType<string>());
    }

    [Property]
    public void Map2_WhenSecondNone_ReturnsNone(int value)
    {
        Maybe.Map2(
                Some.With(value),
                None.OfType<decimal>(),
                (v1, v2) => $"{v1} : {v2}")
            .Should()
            .Be(None.OfType<string>());
    }

    [Property]
    public void Map3_WhenAllSome_ReturnsSome(int value, decimal otherValue, char yetAnotherValue)
    {
        Maybe.Map3(
                Some.With(value),
                Some.With(otherValue),
                Some.With(yetAnotherValue),
                (v1, v2, v3) => $"{v1} : {v2} : {v3}")
            .Should()
            .Be(Some.With($"{value} : {otherValue} : {yetAnotherValue}"));
    }

    [Property]
    public void Map3_WhenFirstNone_ReturnsNone(decimal otherValue, char yetAnotherValue)
    {
        Maybe.Map3(
                None.OfType<int>(),
                Some.With(otherValue),
                Some.With(yetAnotherValue),
                (v1, v2, v3) => $"{v1} : {v2} : {v3}")
            .Should()
            .Be(None.OfType<string>());
    }

    [Property]
    public void Map3_WhenSecondNone_ReturnsNone(int value, char yetAnotherValue)
    {
        Maybe.Map3(
                Some.With(value),
                None.OfType<decimal>(),
                Some.With(yetAnotherValue),
                (v1, v2, v3) => $"{v1} : {v2} : {v3}")
            .Should()
            .Be(None.OfType<string>());
    }

    [Property]
    public void Map3_WhenThirdNone_ReturnsNone(int value, decimal otherValue)
    {
        Maybe.Map3(
                Some.With(value),
                Some.With(otherValue),
                None.OfType<char>(),
                (v1, v2, v3) => $"{v1} : {v2} : {v3}")
            .Should()
            .Be(None.OfType<string>());
    }
}
