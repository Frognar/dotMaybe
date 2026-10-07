using FsCheck.Xunit;
using static DotMaybe.Prelude;

namespace DotMaybe.PropertyTests;

/// <summary>
/// D18: for a value type <c>T</c>, working with a maybe allocates nothing. Every delegate below is a <c>static</c>
/// lambda, which the compiler caches, so any allocation comes from the library.
/// </summary>
/// <remarks>
/// Skipped in Debug builds (see <see cref="Allocations.SkipInDebug"/>); CI runs them in Release.
/// Not covered on purpose: <c>ToString</c> (a string), sequences (<c>Choose</c>, <c>Sequence</c>, <c>FirstOrNone</c>
/// over <c>IEnumerable&lt;T&gt;</c> allocate like LINQ), JSON, and async steps that really wait.
/// </remarks>
public sealed class AllocationProperties
{
    [Property(Skip = Allocations.SkipInDebug)]
    public void Reading_allocates_nothing(Maybe<int> maybe)
    {
        Allocations.AssertNone("Match", () => _ = maybe.Match(static value => value, static () => 0));
        Allocations.AssertNone("OrDefault", () => _ = maybe.OrDefault(0));
        Allocations.AssertNone("OrDefault(factory)", () => _ = maybe.OrDefault(static () => 0));
        Allocations.AssertNone("Fold", () => _ = maybe.Fold(1, static (state, value) => state + value));
        Allocations.AssertNone("Iter", () => maybe.Iter(static _ => { }));
        Allocations.AssertNone("Tap", () => _ = maybe.Tap(static _ => { }));
        Allocations.AssertNone("ToNullable", () => _ = maybe.ToNullable());
        Allocations.AssertNone("switch", () => _ = maybe switch { int value => value, None => 0 });
    }

    [Property(Skip = Allocations.SkipInDebug)]
    public void Transforming_allocates_nothing(Maybe<int> maybe)
    {
        Allocations.AssertNone("Map", () => _ = maybe.Map(static value => value + 1));
        Allocations.AssertNone("Bind", () => _ = maybe.Bind(static value => Half(value)));
        Allocations.AssertNone("Filter", () => _ = maybe.Filter(static value => value > 0));
        Allocations.AssertNone("Select", () => _ = maybe.Select(static value => value + 1));
        Allocations.AssertNone("Where", () => _ = maybe.Where(static value => value > 0));
        Allocations.AssertNone("SelectMany", () => _ = maybe.SelectMany(static value => Half(value)));
        Allocations.AssertNone(
            "SelectMany(resultSelector)",
            () => _ = maybe.SelectMany(static value => Half(value), static (value, half) => value + half));
    }

    [Property(Skip = Allocations.SkipInDebug)]
    public void Query_syntax_allocates_nothing(Maybe<int> maybe) =>
        // No let, and no where after a second from: those make the compiler allocate anonymous objects, whatever the
        // library does.
        Allocations.AssertNone(
            "query",
            () => _ =
                from value in maybe
                where value > 0
                from half in Half(value)
                select value + half);

    [Property(Skip = Allocations.SkipInDebug)]
    public void Combining_allocates_nothing(Maybe<int> maybe, Maybe<int> other)
    {
        var nested = maybe.Map(static value => Maybes.Some(value));

        Allocations.AssertNone("OrElse", () => _ = maybe.OrElse(other));
        Allocations.AssertNone("OrElse(factory)", () => _ = maybe.OrElse(static () => Maybes.Some(1)));
        Allocations.AssertNone("Zip", () => _ = maybe.Zip(other));
        Allocations.AssertNone("Zip(combine)", () => _ = maybe.Zip(other, static (a, b) => a + b));
        Allocations.AssertNone("Flatten", () => _ = nested.Flatten());
    }

    [Property(Skip = Allocations.SkipInDebug)]
    public void Comparing_allocates_nothing(Maybe<int> maybe, Maybe<int> other)
    {
        Allocations.AssertNone("Equals", () => _ = maybe.Equals(other));
        Allocations.AssertNone("==", () => _ = maybe == other);
        Allocations.AssertNone("GetHashCode", () => _ = maybe.GetHashCode());
        Allocations.AssertNone("CompareTo", () => _ = maybe.CompareTo(other));
    }

    [Property(Skip = Allocations.SkipInDebug)]
    public void Interop_allocates_nothing(int number, int? nullable)
    {
        var text = number.ToString(System.Globalization.CultureInfo.InvariantCulture);
        var dictionary = new Dictionary<int, int> { [number] = number };

        Allocations.AssertNone("FromNullable", () => _ = Maybe.FromNullable(nullable));
        Allocations.AssertNone(
            "Parse",
            () => _ = Maybe.Parse<int>(text, System.Globalization.CultureInfo.InvariantCulture));
        Allocations.AssertNone("GetValueOrNone", () => _ = dictionary.GetValueOrNone(number));
    }

    [Fact(Skip = Allocations.SkipInDebug)]
    public void Async_steps_on_None_allocate_nothing()
    {
        var empty = Maybes.Empty<int>();

        Allocations.AssertNone("MapAsync", () => _ = empty.MapAsync(static value => Task.FromResult(value)).IsCompleted);
        Allocations.AssertNone(
            "BindAsync",
            () => _ = empty.BindAsync(static value => Task.FromResult(Maybes.Some(value))).IsCompleted);
        Allocations.AssertNone(
            "FilterAsync",
            () => _ = empty.FilterAsync(static value => Task.FromResult(value > 0)).IsCompleted);
        Allocations.AssertNone(
            "MatchAsync",
            () => _ = empty.MatchAsync(static value => Task.FromResult(value), static () => 0).IsCompleted);
    }

    [Property(Skip = Allocations.SkipInDebug)]
    public void Async_steps_on_completed_sources_with_synchronous_steps_allocate_nothing(Maybe<int> maybe)
    {
        var fromValueTask = ValueTask.FromResult(maybe);
        var fromTask = Task.FromResult(maybe);

        Allocations.AssertNone(
            "chain on ValueTask",
            () => _ = fromValueTask
                .MapAsync(static value => value + 1)
                .BindAsync(static value => Half(value))
                .FilterAsync(static value => value > 0)
                .OrDefaultAsync(0)
                .IsCompleted);
        Allocations.AssertNone(
            "chain on Task",
            () => _ = fromTask.MapAsync(static value => value + 1).OrDefaultAsync(0).IsCompleted);
        Allocations.AssertNone(
            "async query",
            () => _ = (from value in fromValueTask where value > 0 select value + 1).IsCompleted);
    }

    private static Maybe<int> Half(int value) => value % 2 == 0 ? value / 2 : none;
}
