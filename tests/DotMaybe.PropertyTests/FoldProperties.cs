using FsCheck.Xunit;

namespace DotMaybe.PropertyTests;

/// <summary>
/// <c>Fold</c> treats a maybe as a collection of zero or one values: the folder runs once on a value, and without
/// one the state comes back unchanged.
/// </summary>
public sealed class FoldProperties
{
    [Property]
    public void A_value_is_folded_into_the_state(int value, string state, Func<string, int, string> folder) =>
        Assert.Equal(folder(state, value), Maybes.Some(value).Fold(state, folder));

    [Property]
    public void None_gives_the_state_unchanged(int state)
    {
        var calls = 0;

        var result = Maybes.Empty<string>().Fold(state, (current, _) =>
        {
            calls++;
            return current + 1;
        });

        Assert.Equal(state, result);
        Assert.Equal(0, calls);
    }

    [Property]
    public void Fold_is_Match_with_the_state(Maybe<int> maybe, long state, Func<long, int, long> folder) =>
        Assert.Equal(maybe.Match(value => folder(state, value), () => state), maybe.Fold(state, folder));

    [Property]
    public void The_folder_runs_once_for_a_value(int value)
    {
        var calls = 0;

        _ = Maybes.Some(value).Fold(0, (current, next) =>
        {
            calls++;
            return current + next;
        });

        Assert.Equal(1, calls);
    }

    [Property]
    public void Counting_gives_zero_or_one(Maybe<string> maybe) =>
        Assert.Equal(maybe.Match(_ => 1, () => 0), maybe.Fold(0, (count, _) => count + 1));

    [Property]
    public void Fold_agrees_with_folding_a_sequence_of_zero_or_one_values(Maybe<int> maybe, int seed)
    {
        var values = maybe.Match(value => new[] { value }, () => Array.Empty<int>());

        Assert.Equal(values.Aggregate(seed, (sum, value) => sum + value), maybe.Fold(seed, (sum, value) => sum + value));
    }

    [Property]
    public void A_null_state_is_allowed(Maybe<int> maybe)
    {
        string? state = null;

        var result = maybe.Fold(state, (current, value) => current);

        Assert.Null(result);
    }

    [Property]
    public void A_missing_folder_is_rejected_whatever_the_case(Maybe<int> maybe)
    {
        var error = Assert.Throws<ArgumentNullException>(() => maybe.Fold(0, (Func<int, int, int>)null!));

        Assert.Equal("folder", error.ParamName);
    }
}
