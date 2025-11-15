# GitVersion Configuration

This project uses [GitVersion](https://gitversion.net/) to automatically calculate semantic versions based on Git history.

## How it works

GitVersion analyzes your Git repository and generates a version number based on:
- Branch names
- Commit messages
- Git tags

## Version Strategy

- **main** branch: Production versions (e.g., `1.2.3`)
- **develop** branch: Alpha versions (e.g., `1.2.3-alpha.1`)
- **feature/** branches: Beta versions (e.g., `1.2.3-beta.feature-name.1`)
- **pull-request/** branches: PR versions (e.g., `1.2.3-pr.123.1`)

## Controlling Version Bumps

You can control version increments using commit messages:

- `+semver: major` or `+semver: breaking` - Bump major version (1.0.0 ? 2.0.0)
- `+semver: minor` or `+semver: feature` - Bump minor version (1.0.0 ? 1.1.0)
- `+semver: patch` or `+semver: fix` - Bump patch version (1.0.0 ? 1.0.1)
- `+semver: none` or `+semver: skip` - Don't bump version

### Examples

```bash
git commit -m "feat: Add new LED effect +semver: minor"
git commit -m "fix: Correct brightness calculation +semver: patch"
git commit -m "BREAKING CHANGE: Remove deprecated API +semver: major"
```

## Creating Releases

To create a new release:

1. Tag your commit on the main branch:
   ```bash
   git tag v1.0.0
   git push origin v1.0.0
 ```

2. The GitHub Action will automatically:
 - Calculate the version using GitVersion
   - Build the Docker image
   - Tag it with multiple versions (e.g., `1.0.0`, `1.0`, `1`, `latest`)
   - Push to GitHub Container Registry

## Local Version Calculation

To see what version GitVersion would calculate locally:

```bash
# Install GitVersion (one-time setup)
dotnet tool install --global GitVersion.Tool

# Calculate version
dotnet gitversion

# Get just the SemVer
dotnet gitversion /showvariable SemVer
```

## Configuration

The GitVersion configuration is in `GitVersion.yml` at the root of the repository. See [GitVersion documentation](https://gitversion.net/docs/) for customization options.
