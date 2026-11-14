# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project

dotMaybe 2.0: a single-package `Maybe<T>` library for .NET 11 / C# 15, built on C# **union types**. The SDK is
pinned in `global.json` (11.0.100 GA, `rollForward: latestFeature`); C# 15 is the default language version for
`net11.0`, so no `LangVersion` is set.
Tests run on Microsoft Testing Platform (xUnit v3), so `dotnet test` takes `--project`/`--solution`, not a bare path.

## Commands

```bash
dotnet restore dotMaybe.slnx --locked-mode              # what CI does; lock files must be up to date
dotnet build dotMaybe.slnx
dotnet test --solution dotMaybe.slnx
dotnet test --solution dotMaybe.slnx -c Release         # allocation (D18) properties are skipped in Debug
dotnet format whitespace --folder --verify-no-changes   # CI formatting check
dotnet build src/DotMaybe/DotMaybe.csproj -c Release -p:LangVersion=14   # CI: no union conversions in src (Stryker)

# Single class / method (xUnit v3 MTP filters, wildcards allowed)
dotnet test --project tests/DotMaybe.PropertyTests/DotMaybe.PropertyTests.csproj --filter-class "*MapProperties"
dotnet test --project tests/DotMaybe.PropertyTests/DotMaybe.PropertyTests.csproj --filter-method "*<MethodName>"

# Nightly profile: every property with 10 000 cases
dotnet build tests/DotMaybe.PropertyTests/DotMaybe.PropertyTests.csproj -c Release -p:PropertyTestProfile=Nightly
dotnet test --project tests/DotMaybe.PropertyTests/DotMaybe.PropertyTests.csproj -c Release --no-build

# After changing Directory.Packages.props or adding a project: refresh and commit packages.lock.json
dotnet restore dotMaybe.slnx --force-evaluate

# Record new public API in PublicAPI.Unshipped.txt (RS0016 otherwise fails the build)
dotnet format analyzers src/DotMaybe/DotMaybe.csproj --diagnostics RS0016 --severity warn

# Mutation testing (Stryker), benchmarks (manual only), Native AOT smoke test
dotnet tool restore && (cd tests/DotMaybe.PropertyTests && dotnet stryker)
dotnet run -c Release --project benchmarks/DotMaybe.Benchmarks -- --filter '*Chain*' --inProcess  # --inProcess required: BDN 0.15.8 doesn't recognize .NET 11
dotnet publish smoke/DotMaybe.AotSmoke/DotMaybe.AotSmoke.csproj -c Release -r linux-x64 -o artifacts/aot && ./artifacts/aot/DotMaybe.AotSmoke
```

## Architecture

### `src/DotMaybe` — one `readonly partial struct Maybe<T>` split by concern

- `Maybe.cs`: the core. `[Union]` struct with private `_value`/`_isSome`, implementing the nested
  `Maybe<T>.IUnionMembers` interface (static `Create(T)`/`Create(None)`, `Value`, `TryGetValue` overloads) that the
  C# compiler uses for union conversions and pattern matching. These members are implemented **explicitly** so they
  stay out of the public API. Also holds `Match`, `OrDefault`, `Map`, `Bind`, `Filter`, LINQ query methods
  (`Select`/`SelectMany`/`Where`), equality, and the internal `TryGetSome` and `Of` used throughout library code.
- Other partials: `Maybe.Combinators.cs` (OrElse, Zip, Fold, Iter, Tap), `Maybe.Async.cs` (`*Async` returning
  `ValueTask`), `Maybe.Ordering.cs` (`IComparable`, None first), `Maybe.Json.cs` + `Json/` (`MaybeJsonConverter<T>`;
  `INestedMaybe` detects `Some(None)`, which throws on write).
- Static/extension classes: `Maybe.Static.cs` (`Maybe.Flatten`), `Maybe.Interop.cs` (`Maybe.FromNullable`,
  `Maybe.Parse`), `Maybe.Collections.cs`
  (Choose, Traverse, Sequence, FirstOrNone, …), `MaybeNullableExtensions.cs`, `MaybeTaskExtensions.cs` and
  `MaybeTaskQueryExtensions.cs` (pipelines and query syntax over `Task<Maybe<T>>`/`ValueTask<Maybe<T>>`).
- `None.cs` is the unit case type; `Prelude.none` is the lower-case value (deliberately lower-case to avoid
  ambiguity with the type `None` in patterns).

### Design invariants (enforced by tests and analyzers)

- **No invalid states:** `default(Maybe<T>)` is None; a `null` reaching `Create`, `Map`, `Zip` etc. becomes None.
  `T : notnull` everywhere.
- **No value accessors / no throwing for missing values.** Values come out via pattern matching, `Match`,
  `OrDefault`, or total functions. Delegates are null-checked with `ArgumentNullException.ThrowIfNull`.
- **No `<`/`>` operators** on purpose (CA1036 suppressed): `maybeAge >= 18` must not compile.
- **D18 zero allocation** for value-type `T` in Release builds: library code must not use capturing lambdas
  (e.g. `Bind(v => other.Map(...))`); read fields or use `TryGetSome`, and for async steps use `static async` local
  functions (the `...After` pattern in `MaybeTaskExtensions.cs`). `AllocationProperties` checks it.
- **Async (D32/D33):** async helpers catch delegate exceptions and surface them through the `ValueTask`; on None
  nothing runs and the result is already complete.
- **AOT/trimming clean** (`IsAotCompatible`), `AnalysisLevel latest-all` with `TreatWarningsAsErrors`; deliberate
  rule exceptions live in `.editorconfig` with their reasons. Every public API change must be reflected in
  `PublicAPI.Unshipped.txt` (RS0016/RS0017 fail the build); shipped lines move to `PublicAPI.Shipped.txt` after a
  release, and changing a shipped line is a breaking change.

### Tests

- `tests/DotMaybe.PropertyTests`: FsCheck `[Property]` tests. `AssemblyInfo.cs` registers `MaybeArbitraries`
  for all properties (100 cases, or 10 000 with `NIGHTLY`). Generic test code builds maybes via the
  `Maybes` helper and reads them with `maybe is T value` (allowed since the .NET 11 GA compiler; the RC compiler
  rejected it with CS8780). A failing property prints
  a seed; pin it with `[Property(Replay = "seed1,seed2")]`. Allocation tests use `Allocations` with
  `Skip = Allocations.SkipInDebug` and `static` lambdas.
- `tests/DotMaybe.CompilerContractTests`: compiles consumer snippets (`Snippet.Compile(Consumer.File(...))`) with
  the **SDK's own Roslyn** and asserts on diagnostics via `ContractAssert` (`CompilesCleanly`, `FailsWith`,
  `WarnsSwitchIsNotExhaustive`, …). They pin how C# unions behave with `Maybe<T>`; run them first after bumping
  the SDK in `global.json`. Must be built with the dotnet CLI (needs `RoslynCoreAssembliesPath`).
- Test method names are sentence-like with underscores (CA1707 disabled under `tests/`).
- `src/` changes are mutation-tested nightly; a surviving mutant means a missing property.
- Library code builds maybes with the internal `Maybe<T>.Of(value)` and `default` (None), never with union
  conversions (`return value;`, `: none`) or union patterns: Stryker's bundled Roslyn still treats unions as a
  preview feature (CS8652), fails to compile the mutated library and reports 0 %. CI checks it by building the
  library as C# 14 (`dotnet build src/DotMaybe/DotMaybe.csproj -c Release -p:LangVersion=14`).
- Test types that hold maybes in fields (JSON models such as `Profile`) are `internal`: xUnit's discovery loads every
  public type of the test assembly, which loads the library before Stryker swaps in the mutated build, and every test
  host crashes (`FileNotFoundException` for `DotMaybe, Version=2.0.0.0`; no score).

## Workflow

- Squash-merged PRs; the **PR title must be a Conventional Commit** (`feat:`, `fix:`, `test:`, `refactor:`,
  `docs:`, `build:`, `ci:`, `chore:`) — release-please derives the version and changelog from it. Versions come
  from git tags via MinVer; don't edit version numbers by hand (prerelease phase changes go through `release-as` in
  `release-please-config.json`).
- Features are built test-first from "test packages" (`TP-nn`, branch `feat/tp-nn-<topic>`): failing tests plus API
  stubs throwing `NotImplementedException`, then implement until green.
- Package versions are centrally managed in `Directory.Packages.props`; every project has a committed
  `packages.lock.json`.
