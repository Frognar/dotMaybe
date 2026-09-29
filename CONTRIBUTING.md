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

Before merging the first release PR, check the Trusted Publishing policy on nuget.org. A new policy can start as
*temporarily active* for 7 days and only becomes permanent after the first successful publish. If the 7 days have
passed, restart the window on the policy list, then merge.

The release PR is opened by `GITHUB_TOKEN`, so CI does not run on it and its required checks never report.
Merge it with the ruleset bypass (see below), or set up `RELEASE_PLEASE_TOKEN` instead.

Moving to the next pre-release phase or to the final release: pull requests are squash-merged with the title as the
commit message, so a `Release-As:` footer would be lost. Set the version in `release-please-config.json` instead:

```json
"prerelease-type": "beta",
"packages": { ".": { "changelog-path": "CHANGELOG.md", "release-as": "2.0.0-beta.1" } }
```

After that release is published, remove `release-as` in a follow-up PR; later versions then count up on their own
(`2.0.0-beta.2`, ...). For `2.0.0` itself, set `"release-as": "2.0.0"` and `"prerelease": false`.

## Public API

The public API of `src/DotMaybe` is recorded in `PublicAPI.Shipped.txt` and `PublicAPI.Unshipped.txt`
(Microsoft.CodeAnalysis.PublicApiAnalyzers). A public member that is not listed fails the build (RS0016), and so does
a listed member that no longer exists (RS0017). Every API change therefore shows up in the pull request diff.

- New or changed API goes to `PublicAPI.Unshipped.txt`. Let the tooling write the lines:
  `dotnet format analyzers src/DotMaybe/DotMaybe.csproj --diagnostics RS0016 --severity warn`
  (or the IDE's *Add to public API* fix for the whole project).
- After a release, move the lines from `PublicAPI.Unshipped.txt` to `PublicAPI.Shipped.txt`.
- Removing or changing a shipped line is a breaking change: it needs a major version.

The library also builds with `AnalysisLevel latest-all`. Rules that conflict with deliberate design decisions are
switched off in `.editorconfig`, each with its reason.

## Dependencies and lock files

Every project has a `packages.lock.json`, and CI restores with `--locked-mode`, so a dependency changes only through
a commit. After changing a version in `Directory.Packages.props` or adding a project, refresh the lock files and
commit them:

```bash
dotnet restore dotMaybe.slnx --force-evaluate
```

Dependabot updates the lock files together with the versions.

## Updating the .NET SDK (RC1 → RC2 → GA)

1. Bump `sdk.version` in `global.json` to the exact SDK version.
2. Read the C# release notes for changes to unions.
3. Run the compiler contract tests first. They are the ones that notice changes in union behavior.
4. Update test dependencies if needed.

## One-time repository setup

1. **Pull requests** (Settings → General → Pull Requests):
   - allow squash merging only;
   - default squash commit message: *Pull request title* (release-please reads that commit message);
   - automatically delete head branches.
2. **Actions** (Settings → Actions → General): allow GitHub Actions to create and approve pull requests
   (release-please opens the release PR).
3. **Environment `release`** (Settings → Environments):
   - required reviewers: yourself, with *Prevent self-review* off;
   - deployment branches: `main` only. Not tags: the publish job runs in the workflow started by the push to `main`;
   - environment secret `NUGET_USER`: your nuget.org profile name (not the e-mail address).
4. **nuget.org Trusted Publishing policy:** owner `Frognar`, repository `dotMaybe`, workflow `release.yml`,
   environment `release`, packages `dotMaybe`. See *Releases* for the 7-day activation window.
5. **Security** (Settings → Code security): private vulnerability reporting and Dependabot alerts on.
6. **Ruleset for `main`** (Settings → Rules → Rulesets):
   - bypass list: *Repository admin*, for pull requests only (to merge the release PR);
   - require a pull request, 0 approvals, squash only;
   - require the status checks `Build and test (ubuntu-latest)` and `Conventional Commits title`
     (the second one can be selected once a first pull request has run it);
   - require linear history and signed commits;
   - block force pushes and deletion.
7. **Optional secret `RELEASE_PLEASE_TOKEN`:** a fine-grained token for this repository with *Contents* and
   *Pull requests* read/write. With it, the release PR triggers CI like any other PR and no bypass is needed.
   Tokens expire, so this needs renewing.
