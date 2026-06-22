# RevitSpoolCopy

Autodesk Revit addin (2025-2027) that copies "Assembly Name" parameter values to the Spool field on selected MEP fabrication parts.

## Quick Start

### Build
```powershell
dotnet build RevitSpoolCopy.csproj -c Release
```
Revit API assemblies come from NuGet (reference-only) — no local Revit install needed to build.

### Install
```powershell
.\deploy.ps1                # All installed Revit versions
.\deploy.ps1 -Versions 2026 # Specific version
```
Then restart Revit. Button appears in **Spool Tools** ribbon tab.

## Commands (Spool Tools ribbon tab)
- **Assembly Name → Spool** — copy each selected part's Assembly Name into the native Spool
  field, and mirror it into a readable "Spool" shared parameter (visible in Properties).
- **Map Parameters** — map any source parameter to any target on selected parts (saved for reuse).
- **Batch Ops** — on the current selection: clear / set / report Spool. On spools chosen from
  the whole model:
  - **Export selected spools to MAJ** — one `.MAJ` Fabrication job file per spool.
  - **Create publish set + isolated 3D view** — one named 3D view of all selected spools' parts,
    added to a View/Sheet Set.
- **Spool Manager** — list, rename, or delete spools across the model.

## Usage (Assembly Name → Spool)
1. Select one or more fabrication parts in model
2. Click **Assembly Name → Spool** button
3. Dialog confirms # copied & # skipped

## Diagnostics
Runtime activity and errors are logged to
`%APPDATA%\Autodesk\Revit\Addins\RevitSpoolCopy\log.txt`.

## Documentation
- [spec.md](spec.md) — Feature spec & architecture
- [Claude.md](Claude.md) — Dev notes & build walkthrough
- [CHANGELOG.md](CHANGELOG.md) — Release history

## License
MIT

## Author
Mike Chandler
