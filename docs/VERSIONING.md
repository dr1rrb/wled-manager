# Versioning Strategy

The repository relies on [Nerdbank.GitVersioning (NBGV)](https://github.com/dotnet/Nerdbank.GitVersioning) to calculate semantic versions from the Git graph. All configuration lives in `version.json`, and MSBuild consumes it through the `Nerdbank.GitVersioning` package reference in `src/WledManager/WledManager.csproj`.

## How versions are produced

- `version.json` defines a base `version` that you bump whenever a new release train starts.
- `main` (and tags that match `v*`) are treated as public release refs, so they emit clean versions such as `1.2.3`.
- Any other branch automatically receives a `preview` prerelease suffix whose number equals the git height since the last base bump (for example `1.2.3-preview.5`).
- CI and local builds run the `nbgv` CLI, ensuring Docker tags, assemblies, and NuGet metadata stay in sync.

## Working locally

```powershell
# Install the CLI once
dotnet tool install --global nbgv

# Show every calculated field in JSON
dotnet nbgv get-version -f json

# Friendly helper (installs the CLI if needed)
./build/get-version.ps1
```

Use `./build/get-version.ps1 -Variable NuGetPackageVersion` to pull a specific field, or `-ShowAll` to dump the entire JSON payload.

## Bumping the base version

1. Edit the `version` value inside `version.json` (e.g., from `0.1` to `0.2`).
2. Commit the change on the branch that will generate the release.
3. (Optional) Run `dotnet nbgv prepare-release` for a guided bump/tag workflow.

After the bump, the next commit on `main` emits `0.2.0`, while feature branches show `0.2.0-preview.*`.

## Creating releases

1. Ensure `main` contains the desired code.
2. Tag the release commit: `git tag v1.0.0`.
3. Push the tag: `git push origin v1.0.0`.

The GitHub Action will then:
- Resolve versions with `dotnet nbgv get-version -f json`.
- Build Docker images with tags such as `{version}`, `{major}.{minor}`, `{major}`, `latest`, and branch-specific fallbacks.
- Push everything to GitHub Container Registry using the default token.

## Reference

- Configuration file: `version.json`.
- CLI reference: `dotnet nbgv --help`.
- Helper script: `build/get-version.ps1`.
