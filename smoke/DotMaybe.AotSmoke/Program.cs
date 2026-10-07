using System.Text.Json;
using DotMaybe;
using DotMaybe.AotSmoke;
using static DotMaybe.Prelude;

// Exercises the public API in a Native AOT binary: generics over value and reference types, union conversions and
// patterns, LINQ query syntax, async, comparers, and JSON through a source-generated context.
var failures = new List<string>();

Maybe<int> answer = 42;
Maybe<int> nothing = none;

Check(answer.Map(static x => x + 1).OrDefault(0) == 43, "Map");
Check(nothing.Bind(Half).OrDefault(-1) == -1, "Bind on None");
Check(answer switch { int x => x == 42, None => false }, "pattern matching");
Check((from a in answer from b in Half(a) select a + b).OrDefault(0) == 63, "query syntax");
Check(answer.Zip(answer, static (a, b) => a + b).OrDefault(0) == 84, "Zip");
Check(nothing.OrElse(7).OrDefault(0) == 7, "OrElse");
Check(Maybe<string>.IUnionMembers.Create(null!) == Maybe.FromNullable((string?)null), "null becomes None");

Maybe<int>[] sorted = [3, none, 1];
Array.Sort(sorted);
Check(sorted[0] == nothing && sorted[1] == 1 && sorted[2] == 3, "ordering");

string[] texts = ["1", "x", "3"];
Check(texts.Choose(Maybe.Parse<int>).Sum() == 4, "Choose + Parse");
Check(texts.Traverse(Maybe.Parse<int>).Match(static _ => false, static () => true), "Traverse");
Check(answer.ToNullable() == 42 && Maybe.FromNullable((int?)null) == nothing, "nullable interop");

var doubled = await answer.MapAsync(static async x =>
{
    await Task.Yield();
    return x * 2;
});
Check(doubled.OrDefault(0) == 84, "MapAsync");

var asyncQuery = await (
    from a in Task.FromResult(answer)
    from b in Task.FromResult(Half(a))
    select a + b);
Check(asyncQuery.OrDefault(0) == 63, "async query");

var order = new Order("rush", none);
var json = JsonSerializer.Serialize(order, SmokeJsonContext.Default.Order);
Check(json == """{"Note":"rush","Quantity":null}""", $"JSON write: {json}");
Check(JsonSerializer.Deserialize(json, SmokeJsonContext.Default.Order) == order, "JSON round trip");
Check(
    JsonSerializer.Deserialize("""{"Note":null}""", SmokeJsonContext.Default.Order) == new Order(none, none),
    "JSON missing and null properties");
Check(ThrowsJsonException(static () => JsonSerializer.Serialize(
    Maybe<Maybe<int>>.IUnionMembers.Create(nothing),
    typeof(Maybe<Maybe<int>>),
    SmokeJsonContext.Default)), "JSON refuses Some(None)");

if (failures.Count > 0)
{
    foreach (var failure in failures)
    {
        Console.Error.WriteLine($"FAILED: {failure}");
    }

    return 1;
}

Console.WriteLine("Native AOT smoke test passed.");
return 0;

void Check(bool condition, string what)
{
    if (!condition)
    {
        failures.Add(what);
    }
}

static Maybe<int> Half(int value) => value % 2 == 0 ? value / 2 : none;

static bool ThrowsJsonException(Action action)
{
    try
    {
        action();
        return false;
    }
    catch (JsonException)
    {
        return true;
    }
}
