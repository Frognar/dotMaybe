using DotMaybe;
using FluentAssertions;
using FsCheck.Xunit;

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
}
