using FsCheck.Xunit;

namespace DotMaybe.PropertyTests;

/// <summary>
/// The LINQ names are only an overlay: each one behaves exactly like its functional counterpart, but reports
/// its own parameter names.
/// </summary>
public sealed class LinqProperties
{
    [Property]
    public void Select_is_Map(Maybe<int> maybe, Func<int, string> selector) =>
        Assert.Equal(maybe.Map(selector), maybe.Select(selector));

    [Property]
    public void SelectMany_is_Bind(Maybe<int> maybe, Func<int, Maybe<string>> selector) =>
        Assert.Equal(maybe.Bind(selector), maybe.SelectMany(selector));

    [Property]
    public void SelectMany_with_a_result_selector_is_Bind_then_Map(
        Maybe<int> maybe,
        Func<int, Maybe<string>> selector,
        Func<int, string, bool> resultSelector) =>
        Assert.Equal(
            maybe.Bind(value => selector(value).Map(intermediate => resultSelector(value, intermediate))),
            maybe.SelectMany(selector, resultSelector));

    [Property]
    public void Where_is_Filter(Maybe<int> maybe, Func<int, bool> predicate) =>
        Assert.Equal(maybe.Filter(predicate), maybe.Where(predicate));

    [Property]
    public void The_result_selector_never_runs_when_either_step_has_no_value(
        Maybe<int> maybe,
        Func<int, Maybe<string>> selector)
    {
        var calls = 0;

        _ = maybe.SelectMany(selector, (value, intermediate) =>
        {
            calls++;
            return $"{value}{intermediate}";
        });

        var bothHaveValues = maybe.Bind(selector) != Maybes.Empty<string>();
        Assert.Equal(bothHaveValues ? 1 : 0, calls);
    }

    [Property]
    public void A_null_projection_becomes_None(Maybe<int> maybe)
    {
        MaybeAssert.IsNone(maybe.Select<string>(_ => null!));
        MaybeAssert.IsNone(maybe.SelectMany<int, string>(value => Maybes.Some(value), (_, _) => null!));
    }

    [Property]
    public void A_missing_selector_is_rejected_whatever_the_case(Maybe<int> maybe)
    {
        Assert.Equal("selector", Assert.Throws<ArgumentNullException>(() => maybe.Select<string>(null!)).ParamName);
        Assert.Equal("selector", Assert.Throws<ArgumentNullException>(() => maybe.SelectMany<string>(null!)).ParamName);
        Assert.Equal(
            "selector",
            Assert.Throws<ArgumentNullException>(() => maybe.SelectMany<int, int>(null!, (value, _) => value)).ParamName);
    }

    [Property]
    public void A_missing_result_selector_is_rejected_whatever_the_case(Maybe<int> maybe)
    {
        var error = Assert.Throws<ArgumentNullException>(
            () => maybe.SelectMany<int, int>(value => Maybes.Some(value), null!));

        Assert.Equal("resultSelector", error.ParamName);
    }

    [Property]
    public void A_missing_predicate_is_rejected_whatever_the_case(Maybe<int> maybe)
    {
        var error = Assert.Throws<ArgumentNullException>(() => maybe.Where(null!));

        Assert.Equal("predicate", error.ParamName);
    }
}
