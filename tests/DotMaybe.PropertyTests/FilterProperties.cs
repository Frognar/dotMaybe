using FsCheck.Xunit;
using static DotMaybe.Prelude;

namespace DotMaybe.PropertyTests;

/// <summary>
/// <c>Filter</c> keeps a value only when it satisfies a predicate.
/// </summary>
public sealed class FilterProperties
{
    [Property]
    public void A_value_is_kept_exactly_when_it_satisfies_the_predicate(int value, Func<int, bool> predicate)
    {
        var expected = predicate(value) ? Maybes.Some(value) : Maybes.Empty<int>();

        Assert.Equal(expected, Maybes.Some(value).Filter(predicate));
    }

    [Property]
    public void None_stays_None_and_the_predicate_never_runs(Func<int, bool> predicate)
    {
        var calls = 0;

        var result = Maybes.Empty<int>().Filter(value =>
        {
            calls++;
            return predicate(value);
        });

        Assert.Equal(Maybes.Empty<int>(), result);
        Assert.Equal(0, calls);
    }

    [Property]
    public void The_predicate_runs_exactly_once_on_a_value(int value)
    {
        var calls = 0;

        _ = Maybes.Some(value).Filter(_ =>
        {
            calls++;
            return true;
        });

        Assert.Equal(1, calls);
    }

    [Property]
    public void Always_true_changes_nothing(Maybe<int> maybe) =>
        Assert.Equal(maybe, maybe.Filter(_ => true));

    [Property]
    public void Always_false_gives_None(Maybe<int> maybe) =>
        MaybeAssert.IsNone(maybe.Filter(_ => false));

    [Property]
    public void Filtering_twice_is_filtering_by_both(
        Maybe<int> maybe,
        Func<int, bool> first,
        Func<int, bool> second) =>
        Assert.Equal(
            maybe.Filter(first).Filter(second),
            maybe.Filter(value => first(value) && second(value)));

    [Property]
    public void Filtering_is_idempotent(Maybe<int> maybe, Func<int, bool> predicate) =>
        Assert.Equal(maybe.Filter(predicate), maybe.Filter(predicate).Filter(predicate));

    [Property]
    public void Filter_is_Bind_with_a_condition(Maybe<int> maybe, Func<int, bool> predicate) =>
        Assert.Equal(
            maybe.Bind<int>(value => predicate(value) ? value : none),
            maybe.Filter(predicate));

    [Property]
    public void A_missing_predicate_is_rejected_whatever_the_case(Maybe<int> maybe)
    {
        var error = Assert.Throws<ArgumentNullException>(() => maybe.Filter(null!));

        Assert.Equal("predicate", error.ParamName);
    }
}
