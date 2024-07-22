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
    public void Map2_WhenFirstIsNone_ReturnsNone(decimal otherValue)
    {
        Maybe.Map2(
                None.OfType<int>(),
                Some.With(otherValue),
                (v1, v2) => $"{v1} : {v2}")
            .Should()
            .Be(None.OfType<string>());
    }

    [Property]
    public void Map2_WhenSecondIsNone_ReturnsNone(int value)
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
    public void Map3_WhenFirstIsNone_ReturnsNone(decimal otherValue, char yetAnotherValue)
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
    public void Map3_WhenSecondIsNone_ReturnsNone(int value, char yetAnotherValue)
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
    public void Map3_WhenThirdIsNone_ReturnsNone(int value, decimal otherValue)
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
