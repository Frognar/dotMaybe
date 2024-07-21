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

    [Property]
    public async Task MatchAsync_WhenSome_MatchesSome(int value)
    {
        (await Some.With(value)
                .MatchAsync(() => Task.FromResult("NONE"), v => Task.FromResult(v.ToString())))
            .Should()
            .Be(value.ToString());
    }

    [Fact]
    public async Task MatchAsync_WhenNone_MatchesNone()
    {
        (await None.OfType<int>()
                .MatchAsync(() => Task.FromResult("NONE"), v => Task.FromResult(v.ToString())))
            .Should()
            .Be("NONE");
    }

    [Property]
    public async Task MatchAsync_WhenSome_MatchesSomeAsynchronously(int value)
    {
        (await Some.With(value)
                .MatchAsync(() => "NONE", v => Task.FromResult(v.ToString())))
            .Should()
            .Be(value.ToString());
    }

    [Fact]
    public async Task MatchAsync_WhenNone_MatchesNoneSynchronously()
    {
        (await None.OfType<int>()
                .MatchAsync(() => "NONE", v => Task.FromResult(v.ToString())))
            .Should()
            .Be("NONE");
    }
}
