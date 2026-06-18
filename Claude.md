# RevitSpoolCopy Development Notes

## Codebase Walkthrough

### App.cs
- `IExternalApplication` entry point
- Creates "Spool Tools" ribbon tab + "Fabrication" panel
- Registers "Assembly Name → Spool" push button
- Routes clicks to `CopySpoolCommand`

### CopySpoolCommand.cs
- `IExternalCommand` implementation
- `Execute()`: main command logic
  - Pre-selected elements only (no selection picker)
  - Filter to `FabricationPart` instances
  - Iterate: read Assembly Name (builtin → fallback custom) → write to `SpoolName`
  - Report counts (copied, skipped)
- `GetAssemblyName()`: reads builtin `ASSEMBLY_NAME`, falls back to custom param with same name

### RevitSpoolCopy.csproj
- `net8.0-windows` target (Revit 2025/2026/2027 compatible)
- References Revit 2025 by default; override at build time: `-p:RevitDir="C:\...\Revit 2026"`
- `PlatformTarget: x64` mandatory
- `Nullable: disable` (legacy Revit API)

### deploy.ps1
- Auto-detects Revit 2025/2026/2027 installs
- Builds against first found Revit (or override `RevitDir`)
- Generates `.addin` manifest for each target Revit version
- Manifest points to actual `bin\Release\RevitSpoolCopy.dll`

## Build & Deploy
```powershell
# Build + install for all versions
.\deploy.ps1

# Build + install for Revit 2026 only
.\deploy.ps1 -Versions 2026
```
Then restart Revit; button appears in "Spool Tools" tab.

## Next Steps
1. git init, push to github.com/dopefish2112/RevitSpoolCopy
2. Set up CI/CD for build validation
3. Plan pluginbundle architecture for multi-command extensibility
4. Consider: UI for parameter mapping, logging config, batch ops
