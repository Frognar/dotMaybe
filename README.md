# dotMaybe

[![CI](https://github.com/Frognar/dotMaybe/actions/workflows/ci.yml/badge.svg)](https://github.com/Frognar/dotMaybe/actions/workflows/ci.yml)

A `Maybe<T>` for C# 15 built on union types: exhaustive pattern matching, query syntax, and no invalid states.

> **Status:** 2.0 is a ground-up rewrite for .NET 11 and is under active development. The 1.x line is retired.

```csharp
using DotMaybe;
using static DotMaybe.Prelude;

Maybe<int> maybe = none;
Maybe<string> otherMaybe = "Hello, Monad!";

var text = otherMaybe switch
{
    string value => value,
    None => "nothing",
};
```

- **Union types.** `Maybe<T>` has exactly two cases, `T` and `None`. Values convert implicitly, and a `switch` that handles both is exhaustive.
- **No invalid states.** `default(Maybe<T>)` is `None`, and a `null` never becomes a value.
- **Functional API.** No `.Value` and no throwing accessors: you get a value out through pattern matching or total functions.
- **Query syntax.** `from`, `where`, `let` and `select` work over maybes.

## Usage

Import the namespace, and `Prelude` if you want the lower-case `none`:

```csharp
using DotMaybe;
using static DotMaybe.Prelude;
```

### Creating

```csharp
Maybe<int> answer = 42;           // a value
Maybe<int> nothing = none;        // no value
Maybe<int> parsed = ok ? 42 : none;
Maybe<int> empty = default;       // also none
```

A `null` that slips past nullable analysis becomes `none` as well.

### Getting the value out

```csharp
var text = answer switch
{
    int value => $"The answer is {value}",
    None => "No answer",
};

var same = answer.Match(value => $"The answer is {value}", () => "No answer");
var number = answer.OrDefault(0);
var lazy = answer.OrDefault(() => ExpensiveFallback());
```

In generic code use `Match`: C# does not allow `maybe is T value` when `T` is a type parameter.

### Transforming and chaining

```csharp
static Maybe<int> Parse(string text) => int.TryParse(text, out var number) ? number : none;

Maybe<string> input = "21";

var doubled = input.Bind(Parse).Map(number => number * 2);   // Some(42)
var positive = doubled.Filter(number => number > 0);         // Some(42)
```

When a `Bind` step returns either a value or `none` inline, name the result type,
because the compiler cannot infer it from `value : none`:

```csharp
var even = doubled.Bind<int>(number => number % 2 == 0 ? number : none);
```

### Query syntax

```csharp
var ratio =
    from numerator in Parse("10")
    from denominator in Parse("2")
    where denominator != 0
    select numerator / denominator;   // Some(5)
```

Any step without a value makes the whole query `none`. `orderby`, `group` and `join` are not supported.

### Combining

```csharp
Maybe<int> port = Parse(fromArguments).OrElse(() => Parse(fromEnvironment)).OrElse(8080);

Maybe<int> area = Parse(width).Zip(Parse(height), (w, h) => w * h);   // none unless both parse
Maybe<(int, int)> size = Parse(width).Zip(Parse(height));

Maybe<int> flat = nested.Flatten();                                  // Maybe<Maybe<int>> → Maybe<int>
int total = port.Fold(0, (sum, value) => sum + value);

port.Iter(Console.WriteLine);                                         // side effects only, and only with a value
Maybe<string> shown = port.Tap(Console.WriteLine).Map(p => $":{p}");  // side effect inside a chain
```

### Collections and interop

```csharp
IEnumerable<int> valid = texts.Choose(Maybe.Parse<int>);            // the numbers that parse, lazily
Maybe<IReadOnlyList<int>> all = texts.Traverse(Maybe.Parse<int>);   // every number, or none
Maybe<IReadOnlyList<int>> both = new[] { a, b }.Sequence();         // Maybe<int>[] → all values, or none

Maybe<User> admin = users.FirstOrNone(user => user.IsAdmin);
Maybe<User> owner = users.SingleOrNone(user => user.IsOwner);       // none for zero or several
Maybe<int> age = ages.GetValueOrNone("Ada");

Maybe<int> fromNullable = Maybe.FromNullable(nullableNumber);        // int? → Maybe<int>
string? backToNull = name.ToNullable();                              // Maybe<string> → string?
```

Nothing here throws for a missing value: an empty sequence, a missing key, text that does not parse and `null`
all give `none`.

### Ordering

Maybes compare like `Option` in F#: `none` first, then the values in their own order.

```csharp
Maybe<int>[] scores = [3, none, 1];
var sorted = scores.Order();          // None, Some(1), Some(3)
var best = scores.Max();              // Some(3)
```

There are no `<` or `>=` operators on purpose: `maybeAge >= 18` does not compile instead of quietly treating
`none` as the smallest age. Compare explicitly, for example `maybeAge.Filter(age => age >= 18)`.

### Async

Every operation has an async counterpart that takes Task-returning delegates and returns a `ValueTask`:

```csharp
Maybe<User> user = await userId.BindAsync(id => repository.FindAsync(id, cancellationToken));
string name = await user.MatchAsync(async u => await FormatAsync(u), () => "anonymous");
```

On `none` nothing runs and nothing waits: the result is already complete. Pass cancellation tokens through
the lambda.

Awaitable maybes (`Task<Maybe<T>>`, `ValueTask<Maybe<T>>`) take the same steps, so a pipeline needs one `await`:

```csharp
string name = await repository.FindAsync(id)          // Task<Maybe<User>>
    .MapAsync(user => user.Name)
    .FilterAsync(name => name.Length > 0)
    .OrDefaultAsync("anonymous");
```

Query syntax works over them too. Each `from` may await a `Task<Maybe<T>>` or take a plain `Maybe<T>`, and the
whole query is one `ValueTask<Maybe<T>>`:

```csharp
string report = await (
        from user in repository.FindUserAsync(id)          // Task<Maybe<User>>
        from order in repository.LastOrderAsync(user)      // Task<Maybe<Order>>
        where order.Total > 0
        select $"{user.Name}: {order.Total}")
    .OrDefaultAsync("no orders");
```

## Requirements

.NET 11 (C# 15).

## Building

```bash
dotnet build dotMaybe.slnx
dotnet test --solution dotMaybe.slnx
```

The SDK version is pinned in `global.json`. Tests run on Microsoft Testing Platform.

## License

[MIT](LICENSE)
