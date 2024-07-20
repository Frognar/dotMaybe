using DotMaybe;
using FluentAssertions;
using FsCheck.Xunit;

namespace dotMaybe.Tests.Unit;

public class MaybeMatchingTests
{
    [Property]
    public void Match_WhenSome_MatchesSome(int value)
    {
        Some.With(value)
            .Match(() => "NONE", v => v.ToString())
            .Should()
            .Be(value.ToString());
    }

    [Fact]
    public void Match_WhenNone_MatchesNone()
    {
        None.OfType<int>()
            .Match(() => "NONE", v => v.ToString())
            .Should()
            .Be("NONE");
    }
}
