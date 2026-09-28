namespace DotMaybe.PropertyTests;

/// <summary>
/// Generic assertions about the case a maybe is in.
/// </summary>
internal static class MaybeAssert
{
    public static void HoldsValue<T>(T expected, Maybe<T> actual)
        where T : notnull
    {
        if (Maybes.TryGetValue(actual, out var value))
        {
            Assert.Equal(expected, value);
            return;
        }

        Assert.Fail($"Expected a value ({expected}), but the maybe is {Maybes.Describe(actual)}.");
    }

    public static void IsNone<T>(Maybe<T> actual)
        where T : notnull
    {
        if (actual is None)
        {
            return;
        }

        Assert.Fail($"Expected None, but the maybe is {Maybes.Describe(actual)}.");
    }
}
