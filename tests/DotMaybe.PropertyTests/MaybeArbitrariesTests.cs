using FsCheck;
using FsCheck.Fluent;
using FsCheck.Xunit;

namespace DotMaybe.PropertyTests;

/// <summary>
/// The generator every other property relies on: it must produce both cases and shrink towards None.
/// </summary>
public sealed class MaybeArbitrariesTests
{
    [Fact]
    public void Generates_both_cases()
    {
        var samples = Gen.Sample(Numbers().Generator, 200);

        Assert.Contains(samples, maybe => maybe is None);
        Assert.Contains(samples, maybe => maybe is int);
    }

    [Fact]
    public void None_does_not_shrink() =>
        Assert.Empty(Numbers().Shrinker(Maybes.Empty<int>()));

    [Property]
    public void A_value_shrinks_to_None_first_and_then_to_smaller_values(int value)
    {
        var expected = ArbMap.Default.ArbFor<int>()
            .Shrinker(value)
            .Select(smaller => Maybes.Some(smaller))
            .Prepend(Maybes.Empty<int>());

        Assert.Equal(expected, Numbers().Shrinker(Maybes.Some(value)));
    }

    private static Arbitrary<Maybe<int>> Numbers() =>
        MaybeArbitraries.MaybeOf(ArbMap.Default.ArbFor<int>());
}
