# Changelog

All notable changes to RevitSpoolCopy are documented here.
Format loosely follows [Keep a Changelog](https://keepachangelog.com/);
versions follow [Semantic Versioning](https://semver.org/).

## [Unreleased]

Deployed and in testing; not yet tagged.

### Added
- **Spool-driven export (Batch Operations)** — select a group of spools from the model, then:
  - **Export selected spools to MAJ** — one Autodesk Fabrication `.MAJ` job file per spool
    into a chosen folder (`FabricationPart.SaveAsFabricationJob`). Guarded by a check for a
    loaded Fabrication Configuration.
  - **Create publish set + isolated 3D view** — one 3D view isolated to all selected spools'
    parts (prompted, duplicate-checked view name), saved into a named View/Sheet Set.
- **Readable "Spool" shared parameter** — Copy Assembly Name → Spool now also mirrors the
  value into an auto-created/bound shared parameter so it shows in Properties/schedules/tags.
- **File logger** — startup, per-command begin/end, and full exception stacks written to
  `%APPDATA%\Autodesk\Revit\Addins\RevitSpoolCopy\log.txt`.
- More tests (53 total): `ParameterLogic`, `SpoolExportLogic`, and model classes.

### Fixed
- **"No command registered."** — ribbon buttons routed through a `CommandRouter` whose active
  command was never set. Each command is now its own `IExternalCommand`; `CommandRouter` removed.

### Notes
- Still runtime-untested in Revit: the PrintManager publish-set save path.

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

[Unreleased]: https://github.com/dopefish2112/RevitSpoolCopy/compare/v0.1.0...HEAD
[0.1.0]: https://github.com/dopefish2112/RevitSpoolCopy/releases/tag/v0.1.0
