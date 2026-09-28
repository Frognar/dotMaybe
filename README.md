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

### Async

Every operation has an async counterpart that takes Task-returning delegates and returns a `ValueTask`:

```csharp
Maybe<User> user = await userId.BindAsync(id => repository.FindAsync(id, cancellationToken));
string name = await user.MatchAsync(async u => await FormatAsync(u), () => "anonymous");
```

On `none` nothing runs and nothing waits: the result is already complete. Pass cancellation tokens through
the lambda.

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
