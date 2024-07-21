# dotMaybe - The Maybe Monad for .NET

[![.net workflow](https://github.com/Frognar/dotMaybe/actions/workflows/dotnet.yml/badge.svg?branch=main)](https://github.com/Frognar/dotMaybe/actions/workflows/dotnet.yml)

dotMaybe is a lightweight, intuitive implementation of the Maybe monad for .NET. It simplifies working with optional values, eliminating null reference exceptions and promoting a more functional, declarative programming style.

# Give it a star ⭐ !

If you find this project valuable, please consider giving it a star! Your support helps others discover this work and encourages further development.

# How to use

## Creation

```csharp
Maybe<string> someMaybe = Some.With("Hello, World!");
Maybe<string> noneMaybe = None.OfType<string>();
```

## Retrieving value

```csharp
int someValue = someMaybe.Match(none: () => -1, some: v => v.Length); // 13
int noneValue = noneMaybe.Match(none: () => -1, some: v => v.Length); // -1
```

## Chaining operations

```csharp
someMaybe
    .Map(v => v.Length) // Maybe<int> of 13
    .Bind(v => v > 20 ? Some.With(v) : None.OfType<int>()); // Maybe<int> of None

noneMaybe
    .Map(v => v.Length) // Maybe<int> of None
    .Bind(v => v > 20 ? Some.With(v) : None.OfType<int>()); // Maybe<int> of None
```

## Query syntax

```csharp
// Execute function if all maybes contain value
Maybe<int> finalSomeMaybe =
    from v1 in someMaybe
    from v2 in Some.With("Hello, World!")
    from v3 in Some.With("Hello, World!")
    let v4 = "Hello, World!"
    select v1.Length + v2.Length + v3.Length + v4.Length; // Maybe<int> of 52

// Returns None when any Maybe don't contain value
Result<int> finalNoneMaybe =
    from v1 in noneMaybe
    from v2 in Some.With("Hello, World!")
    from v3 in None.OfType<string>()
    let v4 = "Hello, World!"
    select v1.Length + v2.Length + v3.Length + v4.Length; // Maybe<int> of none
```

# Contribution

If you would like to contribute to this project, check out [CONTRIBUTING](https://github.com/Frognar/dotMaybe/blob/main/CONTRIBUTING.md) file.

# License

This project is licensed under the terms of the [MIT](https://github.com/Frognar/dotMaybe/blob/main/LICENSE) license.
