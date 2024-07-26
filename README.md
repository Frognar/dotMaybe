# dotMaybe - The Maybe Monad for .NET

[![.net workflow](https://github.com/Frognar/dotMaybe/actions/workflows/dotnet.yml/badge.svg?branch=main)](https://github.com/Frognar/dotMaybe/actions/workflows/dotnet.yml)

dotMaybe is a lightweight, intuitive implementation of the Maybe monad for .NET. It simplifies working with optional values, eliminating null reference exceptions and promoting a more functional, declarative programming style.

# Give it a star ⭐ !

If you find this project valuable, please consider giving it a star! Your support helps others discover this work and encourages further development.

# `Maybe<T>` API Documentation

## Overview
The `Maybe<T>` monad represents a value that may or may not exist, encapsulating the concept of optional data. It provides methods to safely handle, transform, and operate on potentially absent values, offering a robust alternative to null references in both synchronous and asynchronous contexts.

---

### Properties

#### IsSome

```csharp
public bool IsSome { get; }
```

Gets a value indicating whether the maybe contains a value.

Example:

```csharp
var maybe = Some.With(42);
if (maybe.IsSome)
{
    Console.WriteLine("Contains value!");
}
```

#### IsNone

```csharp
public bool IsNone { get; }
```

Gets a value indicating whether the maybe is empty.

Example:

```csharp
var maybe = None.OfType<int>();
if (maybe.IsNone)
{
    Console.WriteLine("Is empty!");
}
```

---

### Methods

#### Match

```csharp
public TResult Match<TResult>(Func<TResult> none, Func<T, TResult> some)
```

Executes one of two provided functions based on whether the `Maybe<T>` contains a value or not.

Example:

```csharp
var maybe = Some.With(42);
var message = maybe.Match(
    () => "Is empty",
    value => "Value: " + value);
Console.WriteLine(message);
```

#### MatchAsync

```csharp
public async Task<TResult> MatchAsync<TResult>(Func<Task<TResult>> none, Func<T, Task<TResult>> some)
```

Asynchronously executes one of two provided functions based on whether the `Maybe<T>` contains a value or not.

Example:

```csharp
var maybe = Some.With(42);
var message = await maybe.MatchAsync(
    async () => await Task.FromResult("Is empty"),
    async value => await Task.FromResult("Value: " + value));
Console.WriteLine(message);
```

#### MatchAsync

```csharp
public async Task<TResult> MatchAsync<TResult>(Func<TResult> none, Func<T, Task<TResult>> some)
```

Asynchronously executes one of two provided functions based on whether the `Maybe<T>` contains a value or not, with a synchronous fallback for the None case.

Example:

```csharp
var maybe = Some.With(42);
var message = await maybe.MatchAsync(
    () => "Is empty",
    async value => await Task.FromResult("Value: " + value));
Console.WriteLine(message);
```

#### Map

```csharp
public Maybe<TResult> Map<TResult>(Func<T, TResult> map)
```

Transforms the value inside a Maybe<T> using the provided mapping function, if a value exists.

Example:
```csharp
var maybe = Some.With(42);
var mappedMaybe = maybe.Map(value => value.ToString());
Console.WriteLine(mappedMaybe.Match(
    () => "Empty",
    value => value));
```

#### Bind

```csharp
public Maybe<TResult> Bind<TResult>(Func<T, Maybe<TResult>> bind)
```

Chains Maybe operations by applying a function that returns a new Maybe, allowing for composition of operations that might fail.

Example:
```csharp
var maybe = Some.With(42);
var boundMaybe = maybe.Bind(value => Some.With(value.ToString()));
Console.WriteLine(boundMaybe.Match(
    () => "Empty",
    value => value));
```

#### Fold

```csharp
public TState Fold<TState>(TState state, Func<TState, T, TState> folder)
```

Reduces the `Maybe<T>` to a single value by applying a folder function if a value exists, otherwise returns the initial state.

Example:

```csharp
var maybe = None.OfType<int>();
var foldedResult = maybe.Fold(0, (acc, value) => acc + value);
Console.WriteLine(foldedResult); // Outputs: 0
```

```csharp
var maybe = Some.With(42);
var foldedResult = maybe.Fold(0, (acc, value) => acc + value);
Console.WriteLine(foldedResult); // Outputs: 42
```

```csharp
var maybe = Some.With(42);
var foldedResult = maybe.Fold(10, (acc, value) => acc + value);
Console.WriteLine(foldedResult); // Outputs: 52
```

#### Filter

```csharp
public public Maybe<T> Filter(Func<T, bool> predicate)
```

Applies a predicate to the value in Maybe<T>, returning None if the predicate fails or the value doesn't exist.

Example:

```csharp
var maybe = Some.With(42);
var filteredMaybe = maybe.Filter(value => value < 50);
Console.WriteLine(filteredMaybe.Match(
    () => "Empty",
    value => "Value: " + value)); // Outputs: Value: 42
```

```csharp
var maybe = Some.With(52);
var filteredMaybe = maybe.Filter(value => value < 50);
Console.WriteLine(filteredMaybe.Match(
    () => "Empty",
    value => "Value: " + value)); // Outputs: Empty
```

```csharp
var maybe = None.OfType<int>();
var filteredMaybe = maybe.Filter(value => value < 50);
Console.WriteLine(filteredMaybe.Match(
    () => "Empty",
    value => "Value: " + value)); // Outputs: Empty
```

#### OrDefault

```csharp
public T OrDefault(T defaultValue)
```

Returns the value if it exists, otherwise returns the specified default value.

Example:

```csharp
var maybe = None.OfType<int>();
var value = maybe.OrDefault(100);
Console.WriteLine(value); // Outputs: 100
```

```csharp
var result = Some.With(42);
var value = result.OrDefault(100);
Console.WriteLine(value); // Outputs: 42
```

#### OrDefault
```csharp
public T OrDefault(Func<T> defaultFactory)
```

Returns the value if it exists, otherwise invokes the provided factory function to create a default value.

Example:

```csharp
var maybe = None.OfType<int>();
var value = maybe.OrDefault(() => 100);
Console.WriteLine(value); // Outputs: 100
```

```csharp
var result = Some.With(42);
var value = result.OrDefault(() => 100);
Console.WriteLine(value); // Outputs: 42
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

---

# Contribution

If you would like to contribute to this project, check out [CONTRIBUTING](https://github.com/Frognar/dotMaybe/blob/main/CONTRIBUTING.md) file.

# License

This project is licensed under the terms of the [MIT](https://github.com/Frognar/dotMaybe/blob/main/LICENSE) license.
