# Changelog

All notable changes to RevitSpoolCopy are documented here.
Format loosely follows [Keep a Changelog](https://keepachangelog.com/);
versions follow [Semantic Versioning](https://semver.org/).

## [0.1.0] - 2026-06-18

First tagged release. Single DLL targets Revit 2025/2026/2027 (.NET 8).

### Added
- **Copy Assembly Name → Spool** — copy the Assembly Name parameter to the native
  Spool field on selected MEP fabrication parts.
- **Map Parameters** — map any source parameter to any target parameter, with a WPF
  dialog and JSON-persisted mapping rules.
- **Batch Operations** — clear Spool, set Spool to a value, or report a spool summary.
- **Spool Manager** — list, rename, and delete spools across the model.
- Modular `ICommand` + `CommandRouter` architecture; "Spool Tools" ribbon tab.
- xUnit test suite (33 tests) covering config persistence and core parameter
  read/write/discovery logic via the `ParameterLogic` seam.
- GitHub Actions CI (build + test on push/PR).
- Revit API consumed as NuGet reference assemblies — no local Revit install needed
  to compile.

[0.1.0]: https://github.com/dopefish2112/RevitSpoolCopy/releases/tag/v0.1.0
