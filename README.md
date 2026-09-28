# dotMaybe

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
- **Query syntax** (planned for 2.0.0-alpha.1): `from x in a from y in b where y != 0 select x / y`.

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
