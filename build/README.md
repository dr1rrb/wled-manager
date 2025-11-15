# Docker Build Scripts

This folder contains scripts to build and manage the WledManager Docker image.

## Usage

### Local build
```bash
./build/build-local.ps1
```

### Push to GHCR
```bash
./build/push-ghcr.ps1 -Version "1.0.0"
```

### Build and push in one command
```bash
./build/build-and-push.ps1 -Version "1.0.0"
```

## GitHub Actions Configuration

The GitHub Action is configured to:
- Automatically build on every push to `main`
- Build on pull requests (without pushing)
- Build and tag versions on git tags (format `v*`)
- Support `linux/amd64` and `linux/arm64` architectures
- **Automatically calculate versions using GitVersion**

## Versioning

This project uses [GitVersion](https://gitversion.net/) to automatically calculate semantic versions.

See [docs/VERSIONING.md](../docs/VERSIONING.md) for details on:
- How versions are calculated
- How to control version bumps via commit messages
- How to create releases

### Quick version control

Use these in your commit messages to control versioning:
- `+semver: major` - Breaking changes (1.0.0 ? 2.0.0)
- `+semver: minor` - New features (1.0.0 ? 1.1.0)
- `+semver: patch` - Bug fixes (1.0.0 ? 1.0.1)

## Generated Docker tags

When pushing to `main` branch, images are tagged with:
- `{version}` - Full semantic version (e.g., `1.2.3`)
- `{major}.{minor}` - Major and minor (e.g., `1.2`)
- `{major}` - Major only (e.g., `1`)
- `latest` - Latest version from main branch
- `main` - Build from main branch
- `main-sha-abc123` - Commit SHA

For git tags (`v*`), additional semantic version tags are created.

## Prerequisites

No special configuration is required. GitHub Actions automatically uses the `GITHUB_TOKEN` to authenticate to GHCR.

GitVersion is automatically installed and configured in the CI/CD pipeline.
