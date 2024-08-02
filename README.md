# dotMaybe - The Maybe Monad for .NET

[![.net workflow](https://github.com/Frognar/dotMaybe/actions/workflows/dotnet.yml/badge.svg?branch=main)](https://github.com/Frognar/dotMaybe/actions/workflows/dotnet.yml)

dotMaybe is a lightweight, intuitive implementation of the Maybe monad for .NET. It simplifies working with optional values, eliminating null reference exceptions and promoting a more functional, declarative programming style.

# Give it a star ⭐!

If you find this project valuable, please consider giving it a star! Your support helps others discover this work and encourages further development.

# `Maybe<T>` API Documentation

## Overview
The `Maybe<T>` monad represents a value that may or may not exist, encapsulating the concept of optional data. It provides methods to safely handle, transform, and operate on potentially absent values, offering a robust alternative to null references in both synchronous and asynchronous contexts.

---

## Properties

### IsSome

Gets a value indicating whether the maybe contains a value.

```csharp
public bool IsSome { get; }
```
Example:
```csharp
Maybe<int> maybe = Some.With(42);
if (maybe.IsSome)
{
    Console.WriteLine("Contains value!");
}
```

### IsNone

Gets a value indicating whether the maybe is empty.

```csharp
public bool IsNone { get; }
```
Example:
```csharp
Maybe<int> maybe = None.OfType<int>();
if (maybe.IsNone)
{
    Console.WriteLine("Is empty!");
}
```

---

## Methods

### Match

Executes one of two provided functions based on whether the `Maybe<T>` contains a value or not.

```csharp
public TResult Match<TResult>(Func<TResult> none, Func<T, TResult> some)
```
Example:
```csharp
Maybe<int> maybe = Some.With(42);
string message = maybe.Match(
    () => "Is empty",
    value => "Value: " + value); // "Value: 42"

Maybe<int> maybe = None.OfType<int>();
string message = maybe.Match(
    () => "Is empty",
    value => "Value: " + value); // "Is empty"
```

### MatchAsync

Asynchronously executes one of two provided functions based on whether the `Maybe<T>` contains a value or not.

##### With both asynchronous functions
```csharp
public async Task<TResult> MatchAsync<TResult>(Func<Task<TResult>> none, Func<T, Task<TResult>> some)
```
Example:
```csharp
Maybe<int> maybe = Some.With(42);
string message = await maybe.MatchAsync(
    async () => await Task.FromResult("Is empty"),
    async value => await Task.FromResult("Value: " + value)); // "Value: 42"

Maybe<int> maybe = None.OfType<int>();
string message = await maybe.MatchAsync(
    async () => await Task.FromResult("Is empty"),
    async value => await Task.FromResult("Value: " + value)); // "Is empty"
```

##### With only function for some being asynchronous
```csharp
public async Task<TResult> MatchAsync<TResult>(Func<TResult> none, Func<T, Task<TResult>> some)
```
Example:
```csharp
Maybe<int> maybe = Some.With(42);
string message = await maybe.MatchAsync(
    () => "Is empty",
    async value => await Task.FromResult("Value: " + value)); // "Value: 42"

Maybe<int> maybe = None.OfType<int>();
string message = await maybe.MatchAsync(
    () => "Is empty",
    async value => await Task.FromResult("Value: " + value)); // "Is empty"
```

### Map

Transforms the value inside a Maybe<T> using the provided mapping function, if a value exists.

```csharp
public Maybe<TResult> Map<TResult>(Func<T, TResult> map)
```
Example:
```csharp
Maybe<int> maybe = Some.With(42);
Maybe<string> mappedMaybe = maybe.Map(value => value.ToString()); // Some with "42"

Maybe<int> maybe = None.OfType<int>();
Maybe<string> mappedMaybe = maybe.Map(value => value.ToString()); // None
```

### MapAsync

Asynchronously transforms the value inside a Maybe<T> using the provided mapping function, if a value exists.

```csharp
public Task<Maybe<TResult>> MapAsync<TResult>(Func<T, Task<TResult>> map)
```
Example:
```csharp
Maybe<int> maybe = Some.With(42);
Maybe<string> mappedMaybe = await maybe.MapAsync(
    async value => await Task.FromResult(value.ToString())); // Some with "42"

Maybe<int> maybe = None.OfType<int>();
Maybe<string> mappedMaybe = await maybe.MapAsync(
    async value => await Task.FromResult(value.ToString())); // None
```

### Bind

Chains Maybe operations by applying a function that returns a new Maybe, allowing for composition of operations that might fail.

```csharp
public Maybe<TResult> Bind<TResult>(Func<T, Maybe<TResult>> bind)
```
Example:
```csharp
Maybe<int> maybe = Some.With(42);
Maybe<string> boundMaybe = maybe.Bind(
    value => Some.With(value.ToString())); // Some with "42"

Maybe<int> maybe = Some.With(42);
Maybe<string> boundMaybe = maybe.Bind(
    value => None.OfType<string>()); // None

Maybe<int> maybe = None.OfType<int>();
Maybe<string> boundMaybe = maybe.Bind(
    value => Some.With(value.ToString())); // None

Maybe<int> maybe = None.OfType<int>();
Maybe<string> boundMaybe = maybe.Bind(
    value => None.OfType<string>()); // None
```

### BindAsync

Chains Maybe operations by asynchronously applying a function that returns a new Maybe,
allowing for composition of operations that might fail.

```csharp
public Task<Maybe<TResult>> BindAsync<TResult>(Func<T, Task<Maybe<TResult>>> bind)
```
Example:
```csharp
Maybe<int> maybe = Some.With(42);
Maybe<string> boundMaybe = await maybe.BindAsync(
    async value => await Task.FromResult(Some.With(value.ToString()))); // Some with "42"

Maybe<int> maybe = Some.With(42);
Maybe<string> boundMaybe = await maybe.BindAsync(
    async value => await Task.FromResult(None.OfType<string>())); // None

Maybe<int> maybe = None.OfType<int>();
Maybe<string> boundMaybe = await maybe.BindAsync(
    async value => await Task.FromResult(Some.With(value.ToString()))); // None

Maybe<int> maybe = None.OfType<int>();
Maybe<string> boundMaybe = await maybe.BindAsync(
    async value => await Task.FromResult(None.OfType<string>())); // None
```

### Fold

Reduces the `Maybe<T>` to a single value by applying a folder function if a value exists, otherwise returns the initial state.

```csharp
public TState Fold<TState>(TState state, Func<TState, T, TState> folder)
```
Example:
```csharp
var maybe = None.OfType<int>();
var foldedResult = maybe.Fold(0, (acc, value) => acc + value); // 0

var maybe = Some.With(42);
var foldedResult = maybe.Fold(0, (acc, value) => acc + value); // 42

var maybe = Some.With(42);
var foldedResult = maybe.Fold(10, (acc, value) => acc + value); // 52
```

### Filter

Applies a predicate to the value in Maybe&lt;T&gt;, returning None if the predicate fails or the value doesn't exist.

```csharp
public public Maybe<T> Filter(Predicate<T> predicate)
```
Example:
```csharp
Maybe<int> maybe = Some.With(42);
Maybe<int> filteredMaybe = maybe.Filter(value => value < 50); // Some with 42

Maybe<int> maybe = Some.With(52);
Maybe<int> filteredMaybe = maybe.Filter(value => value < 50); // None

Maybe<int> maybe = None.OfType<int>();
Maybe<int> filteredMaybe = maybe.Filter(value => value < 50); // None
```

### OrDefault

Returns the value if it exists, otherwise returns the specified default value.

```csharp
public T OrDefault(T defaultValue)
```
Example:
```csharp
Maybe<int> maybe = None.OfType<int>();
int value = maybe.OrDefault(100); // 100

Maybe<int> result = Some.With(42);
int value = result.OrDefault(100); // 42
```

### OrDefault

Returns the value if it exists, otherwise invokes the provided factory function to create a default value.

```csharp
public T OrDefault(Func<T> defaultFactory)
```
Example:
```csharp
Maybe<int> maybe = None.OfType<int>();
int value = maybe.OrDefault(() => 100); // 100

Maybe<int> result = Some.With(42);
int value = result.OrDefault(() => 100); // 42
```

---

## Static Methods

### Implicit conversion

Create a Maybe instance containing the specified value. Or None if the value is null.

```csharp
public static implicit operator Maybe<T>(T? value)
```

Example:
```csharp
Maybe<int> maybe = 42; // Some with 42

int? x = null;
Maybe<int> maybe = x; // None
```

### Flatten

Flattens a nested Maybe, reducing Maybe&lt;Maybe&lt;T&gt;&gt; to Maybe&lt;T&gt;.

```csharp
public static Maybe<T> Flatten<T>(this Maybe<Maybe<T>> nested)
```
Example:
```csharp
Maybe<Maybe<int>> nested = 42.ToMaybe().ToMaybe();
Maybe<int> flattened = nested.Flatten();
```

---

## T extensions

### ToMaybe

Convert a value to a Maybe instance.

##### Non-nullable value types and reference types
```csharp
public static Maybe<T> ToMaybe<T>(this T value)
```
Example:
```csharp
Maybe<string> maybe = "Hello, World!".ToMaybe(); // Some with "Hello, World!"

string? x = null;
Maybe<string> maybe = x.ToMaybe(); // None
```

##### Nullable value types
```csharp
public static Maybe<T> ToMaybe<T>(this T? value) where T : struct
```
Example:
```csharp
Maybe<int> maybe = 42.ToMaybe(); // Some with 42

int? x = null;
Maybe<int> maybe = x.ToMaybe(); // None
```

---

## IEnumerable&lt;T&gt; extensions

### FirstOrNone
Returns the first element of a sequence as a Maybe, or an empty Maybe if the sequence contains no elements.

##### Without predicate
```csharp
public static Maybe<T> FirstOrNone<T>(this IEnumerable<T> source)
```
Example:
```csharp
IEnumerable<int> collection = [42];
Maybe<int> maybe = collection.FirstOrNone(); // Some with 42

IEnumerable<int> collection = [];
Maybe<int> maybe = collection.FirstOrNone(); // None
```

##### With predicate
```csharp
public static Maybe<T> FirstOrNone<T>(this IEnumerable<T> source, Func<T, bool> predicate)
```
Example:
```csharp
IEnumerable<int> collection = [42];
Maybe<int> maybe = collection.FirstOrNone(v => v > 40); // Some with 42

IEnumerable<int> collection = [42];
Maybe<int> maybe = collection.FirstOrNone(v => v < 40); // None

IEnumerable<int> collection = [];
Maybe<int> maybe = collection.FirstOrNone(v => v > 0); // None
```

---

## Static Methods on Static Class `Maybe`

### Map2

Combines two Maybe instances using a mapping function.

```csharp
public static Maybe<TResult> Map2<T1, T2, TResult>(
    Maybe<T1> maybe1,
    Maybe<T2> maybe2,
    Func<T1, T2, TResult> map)
```
Example:
```csharp
Maybe<string> result = Maybe.Map3(
    Some.With(1),
    Some.With("Hello there!"),
    (v1, v2) => $"{v1}: {v3}"); // Some with "1: Hello there!"

Maybe<string> result = Maybe.Map2(
    None.OfType<int>(),
    Some.With("Hello there!"),
    (v1, v2) => $"{v1}: {v3}"); // None

Maybe<string> result = Maybe.Map2(
    Some.With(1),
    None.OfType<string>(),
    (v1, v2) => $"{v1}: {v3}"); // None
```

### Map3

Combines three Maybe instances using a mapping function.

```csharp
public static Maybe<TResult> Map3<T1, T2, T3, TResult>(
    Maybe<T1> maybe1,
    Maybe<T2> maybe2,
    Maybe<T3> maybe3,
    Func<T1, T2, T3, TResult> map)
```
Example:
```csharp
Maybe<string> result = Maybe.Map3(
    Some.With(1),
    Some.With("abcd"),
    Some.With("Hello there!"),
    (v1, v2, v3) => $"{v1} ({v2}): {v3}"); // Some with "1 (abcd): Hello there!"

Maybe<string> result = Maybe.Map3(
    None.OfType<int>(),
    Some.With("abcd"),
    Some.With("Hello there!"),
    (v1, v2, v3) => $"{v1} ({v2}): {v3}"); // None

Maybe<string> result = Maybe.Map3(
    Some.With(1),
    None.OfType<string>(),
    Some.With("Hello there!"),
    (v1, v2, v3) => $"{v1} ({v2}): {v3}"); // None

Maybe<string> result = Maybe.Map3(
    Some.With(1),
    Some.With("abcd"),
    None.OfType<string>(),
    (v1, v2, v3) => $"{v1} ({v2}): {v3}"); // None
```

---

## Query syntax

### Select

Transforms the value of a Maybe wrapped in a Task into a new Maybe.

##### With synchronous selector
```csharp
public Maybe<TResult> Select<TResult>(Func<T, TResult> selector)
```
Example:
```csharp
Maybe<string> mappedMaybe =
    from x in Some.With(42)
    select x.ToString(); // Some with "42"

Maybe<string> mappedMaybe =
    from x in None.OfType<int>
    select x.ToString(); // None
```

##### With asynchronous selector
```csharp
public Task<Maybe<TResult>> Select<TResult>(Func<T, Task<TResult>> selector)
```
Example:
```csharp
Maybe<string> mappedMaybe = await (
    from x in Some.With(42)
    select Task.FromResult(x.ToString())); // Some with "42"

Maybe<string> mappedMaybe = await (
    from x in None.OfType<int>
    select Task.FromResult(x.ToString())); // None
```

### SelectMany

Projects the value of a Maybe into a new Maybe, then flattens the result into a single Maybe.

##### With synchronous selectors
```csharp
public Maybe<TResult> SelectMany<TIntermediate, TResult>(
        Func<T, Maybe<TIntermediate>> intermediateSelector,
        Func<T, TIntermediate, TResult> resultSelector)
```
Example:
```csharp
Maybe<string> boundMaybe =
    from v1 in Some.With(1)
    from v2 in Some.With("Hello there!")
    select $"{v1}: {v2}"; // Some with "1: Hello there!"

Maybe<string> boundMaybe =
    from v1 in None.OfType<int>()
    from v2 in Some.With("Hello there!")
    select $"{v1}: {v2}"; // None

Maybe<string> boundMaybe =
    from v1 in Some.With(1)
    from v2 in None.OfType<string>()
    select $"{v1}: {v2}"; // None
```

##### With asynchronous intermediate selector
```csharp
public Task<Maybe<TResult>> SelectMany<TIntermediate, TResult>(
        Func<T, Task<Maybe<TIntermediate>>> intermediateSelector,
        Func<T, TIntermediate, TResult> resultSelector)
```
Example:
```csharp
Maybe<string> boundMaybe = await (
    from v1 in Some.With(1)
    from v2 in Task.FromResult(Some.With("Hello there!"))
    select $"{v1}: {v2}"); // Some with "1: Hello there!"
```

##### With asynchronous result selector
```csharp
public Task<Maybe<TResult>> SelectMany<TIntermediate, TResult>(
        Func<T, Maybe<TIntermediate>> intermediateSelector,
        Func<T, TIntermediate, Task<TResult>> resultSelector)
```
Example:
```csharp
Maybe<string> boundMaybe = await (
    from v1 in Some.With(1)
    from v2 in Some.With("Hello there!")
    select Task.FromResult($"{v1}: {v2}")); // Some with "1: Hello there!"
```

##### With asynchronous intermediate and result selectors
```csharp
public Task<Maybe<TResult>> SelectMany<TIntermediate, TResult>(
        Func<T, Task<Maybe<TIntermediate>>> intermediateSelector,
        Func<T, TIntermediate, Task<TResult>> resultSelector)
```
Example:
```csharp
Maybe<string> boundMaybe = await (
    from v1 in Some.With(1)
    from v2 in Task.FromResult(Some.With("Hello there!"))
    select Task.FromResult($"{v1}: {v2}")); // Some with "1: Hello there!"
```

### Where

```csharp
public Maybe<T> Where(Predicate<T> predicate)
```

Applies a predicate to the value in Maybe<T>, returning None if the predicate fails or the value doesn't exist.

Example:
```csharp
Maybe<int> filteredMaybe =
    from x in Some.With(42)
    where x > 40
    select x; // Some with 42

Maybe<int> filteredMaybe =
    from x in Some.With(42)
    where x < 40
    select x; // None

Maybe<int> filteredMaybe =
    from x in None.OfType<int>()
    where x < 40
    select x; // None
```

---

### Task&lt;Maybe&gt; Extensions

### Select

Transforms the value of a Maybe wrapped in a Task into a new Maybe.

##### With synchronous selector
```csharp
public static Task<Maybe<TResult>> Select<T, TResult>(this Task<Maybe<T>> source, Func<T, Task<TResult>> selector)
```
Example:
```csharp
Maybe<string> mappedMaybe = await (
    from x in Task.FromResult(Some.With(42))
    select Task.FromResult(x.ToString())); // Some with "42"

Maybe<string> mappedMaybe = await (
    from x in Task.FromResult(None.OfType<int>)
    select Task.FromResult(x.ToString())); // None
```

##### With asynchronous selector
```csharp
public static Task<Maybe<TResult>> Select<T, TResult>(this Task<Maybe<T>> source, Func<T, TResult> selector)
```
Example:
```csharp
Maybe<string> mappedMaybe = await (
    from x in Task.FromResult(Some.With(42))
    select x.ToString()); // Some with "42"

Maybe<string> mappedMaybe = await (
    from x in Task.FromResult(None.OfType<int>)
    select x.ToString()); // None
```

### SelectMany

Projects the value of a Maybe wrapped in a Task into a new Maybe, then flattens the result into a single Maybe.

##### With synchronous selectors
```csharp
public static Task<Maybe<TResult>> SelectMany<T, TIntermediate, TResult>(
        this Task<Maybe<T>> source,
        Func<T, Maybe<TIntermediate>> intermediateSelector,
        Func<T, TIntermediate, TResult> resultSelector)
```
Example:
```csharp
Maybe<string> boundMaybe = await (
    from v1 in Task.FromResult(Some.With(1))
    from v2 in Some.With("Hello there!")
    select $"{v1}: {v2}"); // Some with "1: Hello there!"
```

##### With asynchronous intermediate selector
```csharp
public static Task<Maybe<TResult>> SelectMany<T, TIntermediate, TResult>(
        this Task<Maybe<T>> source,
        Func<T, Task<Maybe<TIntermediate>>> intermediateSelector,
        Func<T, TIntermediate, TResult> resultSelector)
```
Example:
```csharp
Maybe<string> boundMaybe = await (
    from v1 in Task.FromResult(Some.With(1))
    from v2 in Task.FromResult(Some.With("Hello there!"))
    select $"{v1}: {v2}"); // Some with "1: Hello there!"
```

##### With asynchronous result selector
```csharp
public static Task<Maybe<TResult>> SelectMany<T, TIntermediate, TResult>(
        this Task<Maybe<T>> source,
        Func<T, Maybe<TIntermediate>> intermediateSelector,
        Func<T, TIntermediate, Task<TResult>> resultSelector)
```
Example:
```csharp
Maybe<string> boundMaybe = await (
    from v1 in Task.FromResult(Some.With(1))
    from v2 in Some.With("Hello there!")
    select Task.FromResult($"{v1}: {v2}")); // Some with "1: Hello there!"
```

##### With asynchronous intermediate and result selectors
```csharp
public static Task<Maybe<TResult>> SelectMany<T, TIntermediate, TResult>(
        this Task<Maybe<T>> source,
        Func<T, Task<Maybe<TIntermediate>>> intermediateSelector,
        Func<T, TIntermediate, Task<TResult>> resultSelector)
```
Example:
```csharp
Maybe<string> boundMaybe = await (
    from v1 in Task.FromResult(Some.With(1))
    from v2 in Task.FromResult(Some.With("Hello there!"))
    select Task.FromResult($"{v1}: {v2}")); // Some with "1: Hello there!"
```

---

# Contribution

If you would like to contribute to this project, check out [CONTRIBUTING](https://github.com/Frognar/dotMaybe/blob/main/CONTRIBUTING.md) file.

# License

This project is licensed under the terms of the [MIT](https://github.com/Frognar/dotMaybe/blob/main/LICENSE) license.
