# Contributing

## Workflow

- Work happens on short-lived branches, merged into `main` through pull requests (squash merge).
- The PR title becomes the commit message, so it must follow [Conventional Commits](https://www.conventionalcommits.org/):
  `feat: ...`, `fix: ...`, `test: ...`, `refactor: ...`, `docs: ...`, `build: ...`, `ci: ...`, `chore: ...`.
- `main` is protected: CI and the PR title check must pass.

### Test packages (TDD)

The library is built test-first from *test packages* (`TP-nn`): failing tests plus API signatures whose bodies
throw `NotImplementedException`.

1. Put the package on a branch `feat/tp-nn-<topic>`.
2. Make the tests green by implementing the bodies.
3. Refactor, then open a PR to `main`.

Property tests use FsCheck (`[Property]`). A failing property prints its seed; pin it with
`[Property(Replay = "seed1,seed2")]` to reproduce. The nightly workflow runs every property with 10 000 cases:

```bash
dotnet build tests/DotMaybe.PropertyTests/DotMaybe.PropertyTests.csproj -c Release -p:PropertyTestProfile=Nightly
dotnet test --project tests/DotMaybe.PropertyTests/DotMaybe.PropertyTests.csproj -c Release --no-build
```

Compiler contract tests (`tests/DotMaybe.CompilerContractTests`) compile consumer snippets with the SDK's own
compiler and check the diagnostics. They pin down how C# unions behave with `Maybe<T>`.

## Releases

Releases are automated with release-please and published to nuget.org with Trusted Publishing.

1. Every push to `main` updates an open *release PR* with the next version and the changelog.
2. Merging the release PR tags the commit (`v2.0.0-alpha.1`, ...) and creates a GitHub release.
3. The `publish` job (environment `release`, needs approval) packs the tagged commit and pushes it to nuget.org.

The package version comes from the git tag through MinVer; nothing else needs editing.

Moving to the next pre-release phase or to the final release: add a footer to a commit on `main`, e.g.

```text
chore: start beta

Release-As: 2.0.0-beta.1
```

For `2.0.0` itself, also set `"prerelease": false` in `release-please-config.json`.

## Updating the .NET SDK (RC1 → RC2 → GA)

1. Bump `sdk.version` in `global.json` to the exact SDK version.
2. Read the C# release notes for changes to unions.
3. Run the compiler contract tests first. They are the ones that notice changes in union behavior.
4. Update test dependencies if needed.

## One-time repository setup

1. **Environment:** Settings → Environments → `release` → Required reviewers: yourself.
2. **Secret `NUGET_USER`:** your nuget.org profile name (not the e-mail address).
3. **nuget.org Trusted Publishing policy:** owner `Frognar`, repository `dotMaybe`, workflow `release.yml`,
   environment `release`.
4. **Actions permissions:** Settings → Actions → General → allow GitHub Actions to create and approve pull requests
   (release-please opens the release PR).
5. **Optional secret `RELEASE_PLEASE_TOKEN`:** a fine-grained token for this repository with *Contents* and
   *Pull requests* read/write. Without it the release PR is opened with `GITHUB_TOKEN`, which does not trigger CI,
   so required checks never report on it.
6. **Ruleset for `main`:**
   - require a pull request;
   - require the status checks `Build and test (ubuntu-latest)` and `Conventional Commits title`;
   - require linear history and signed commits;
   - block force pushes and deletion.

