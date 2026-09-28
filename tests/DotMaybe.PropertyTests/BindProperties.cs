using FsCheck.Xunit;
using static DotMaybe.Prelude;

namespace DotMaybe.PropertyTests;

/// <summary>
/// <c>Bind</c> makes <c>Maybe</c> a monad: steps that can each fail chain without nesting, and the three monad laws
/// hold. "Return" is the union conversion from a value (<c>Maybes.Some</c> here).
/// </summary>
public sealed class BindProperties
{
    [Property]
    public void Left_identity(int value, Func<int, Maybe<string>> bind) =>
        Assert.Equal(bind(value), Maybes.Some(value).Bind(bind));

    [Property]
    public void Right_identity(Maybe<int> maybe) =>
        Assert.Equal(maybe, maybe.Bind(value => Maybes.Some(value)));

    [Property]
    public void Associativity(
        Maybe<int> maybe,
        Func<int, Maybe<string>> first,
        Func<string, Maybe<bool>> second) =>
        Assert.Equal(
            maybe.Bind(first).Bind(second),
            maybe.Bind(value => first(value).Bind(second)));

    [Property]
    public void None_stays_None_and_bind_never_runs(Func<int, Maybe<string>> bind)
    {
        var calls = 0;

        var result = Maybes.Empty<int>().Bind(value =>
        {
            calls++;
            return bind(value);
        });

        Assert.Equal(Maybes.Empty<string>(), result);
        Assert.Equal(0, calls);
    }

    [Property]
    public void Bind_runs_exactly_once_on_a_value(int value)
    {
        var calls = 0;

        _ = Maybes.Some(value).Bind(number =>
        {
            calls++;
            return Maybes.Some(number);
        });

        Assert.Equal(1, calls);
    }

    [Property]
    public void A_step_can_turn_a_value_into_None(Maybe<int> maybe) =>
        MaybeAssert.IsNone(maybe.Bind<string>(_ => none));

    [Property]
    public void Bind_is_Match_without_the_wrapping(Maybe<int> maybe, Func<int, Maybe<string>> bind) =>
        Assert.Equal(maybe.Match(bind, () => Maybes.Empty<string>()), maybe.Bind(bind));

    [Property]
    public void Map_is_Bind_with_a_wrapped_result(Maybe<int> maybe, Func<int, string> map) =>
        Assert.Equal(maybe.Map(map), maybe.Bind(value => Maybes.Some(map(value))));

    [Property]
    public void Binding_the_identity_flattens_one_level(Maybe<Maybe<int>> outer) =>
        Assert.Equal(
            outer.Match(inner => inner, () => Maybes.Empty<int>()),
            outer.Bind(inner => inner));

    [Property]
    public void A_missing_bind_is_rejected_whatever_the_case(Maybe<int> maybe)
    {
        var error = Assert.Throws<ArgumentNullException>(() => maybe.Bind<string>(null!));

        Assert.Equal("bind", error.ParamName);
    }
}
