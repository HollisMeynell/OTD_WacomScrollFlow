# Repository Guidelines

## Project Structure & Module Organization

Production code is in `src/WacomScrollFlow/`, xUnit tests in `tests/WacomScrollFlow.Tests/`, and release packaging in `scripts/Package.ps1`. The tag-triggered release workflow is `.github/workflows/release.yml`. Generated packages belong in the ignored `artifacts/` directory. Keep English user documentation in `README.md` and Chinese documentation in `README-zh.md`.

## Build, Test, and Development Commands

Use .NET 8 SDK (see `global.json`). From the repository root, build with `dotnet build src/WacomScrollFlow/WacomScrollFlow.csproj -c Release` and test with `dotnet test tests/WacomScrollFlow.Tests/WacomScrollFlow.Tests.csproj -c Release`. Package the built DLL with `./scripts/Package.ps1 -Version <project-version>`. Release tags must match the project version, for example `v0.1.2`.

## Coding Style & Naming Conventions

Follow the conventions of the language and framework selected for the implementation. Keep indentation and naming consistent within each file, use descriptive identifiers, and avoid unrelated formatting changes. Add a formatter or linter configuration when the toolchain is established, and document how contributors should run it. Prefer small modules with a clearly defined responsibility.

## Testing Guidelines

xUnit tests cover gesture thresholds and pipeline behavior. Add regression tests for changes to scrolling, button state, pressure suppression, or cursor locking. Use descriptive test names and keep tests independent of local machine settings. No coverage threshold is defined.

## Commit & Pull Request Guidelines

No Git history is available to establish existing commit conventions. Use concise, imperative commit subjects, such as `Add initial project setup`, and keep each commit focused. Pull requests should explain the change, its purpose, and how it was verified. Link relevant issues when available, and include screenshots for visible interface changes. Explicitly identify checks that could not be run.

## Security & Configuration

Never commit credentials, tokens, or machine-specific secrets. Provide example configuration with placeholder values, and exclude generated artifacts and local settings through `.gitignore` when the project tooling is added.
