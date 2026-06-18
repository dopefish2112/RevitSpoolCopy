# RevitSpoolCopy Development Notes

## Architecture: Pluginbundle-Ready

### Folder Structure
```
RevitSpoolCopy/
├── App.cs                      (IExternalApplication, ribbon bootstrap)
├── Commands/
│   ├── ICommand.cs             (interface for all commands)
│   ├── CommandRouter.cs        (IExternalCommand, dispatches to commands)
│   ├── CopyAssemblyNameCommand.cs  (MVP: copy Assembly Name → Spool)
│   ├── MapParametersCommand.cs (Phase 2a: map any param → any param)
│   └── BatchOperationsCommand.cs (stub: coming Phase 2b)
├── Models/
│   ├── FabricationPartHelper.cs (parameter read/write/filter logic)
│   ├── ParameterMapping.cs (mapping rule model)
│   ├── ParameterMappingConfig.cs (JSON persistence)
│   └── ParameterDiscoveryHelper.cs (param discovery & CRUD)
├── UI/
│   └── MapParametersDialog.xaml/.xaml.cs (WPF dialog for mapping selection)
└── deploy.ps1, spec.md, README.md, .gitignore
```

### Key Design Patterns

**ICommand Interface**
- All commands implement `ICommand` for consistency
- `Id`: unique button identifier
- `DisplayName`, `ToolTip`, `LongDescription`: UI metadata
- `Execute(uidoc, message)`: command logic

**CommandRouter (IExternalCommand)**
- Single entry point for all ribbon buttons
- Routes to appropriate ICommand
- MVP: routes all buttons → CopyAssemblyNameCommand
- TODO: enhance with button context or tag to dispatch to different commands

**FabricationPartHelper (Static Utilities)**
- `GetParameterValue(element, paramName)`: read Assembly Name or custom param
- `SetSpoolName(part, value)`: write to native Spool field
- `FilterFabricationParts(elementIds, doc)`: cast list to FabricationPart

**App.cs (Ribbon Bootstrap)**
- Creates "Spool Tools" tab and "Fabrication" panel
- Instantiates all ICommand objects
- AddCommandButton() wires each to CommandRouter
- Clean separation: one button per command

### Build & Deploy
Revit API assemblies come from NuGet (`Nice3point.Revit.Api.RevitAPI/RevitAPIUI`,
reference-only) — no local Revit install needed to compile. This also makes CI work.
```powershell
# Build the add-in
dotnet build -c Release

# Run unit tests
dotnet test RevitSpoolCopy.Tests/RevitSpoolCopy.Tests.csproj

# Build + install for all installed Revit versions
.\deploy.ps1

# Install for specific version
.\deploy.ps1 -Versions 2026
```

Output: `bin\Release\RevitSpoolCopy.dll` (x64, single DLL for 2025/2026/2027)

### CI
`.github/workflows/ci.yml` runs on push/PR to master: restore → build (Release) →
test, on `windows-latest`. No Revit install required (NuGet ref assemblies).

### Phase 1 (Complete)
✅ Refactored to modular, extensible architecture
✅ Created ICommand interface
✅ Extracted FabricationPartHelper utilities
✅ Created command stubs (MapParametersCommand, BatchOperationsCommand)
✅ Single CommandRouter entry point
✅ Build succeeds, single DLL works across Revit versions

### Phase 2 (Complete)
✅ MapParametersCommand with WPF dialog
✅ ParameterMapping model & JSON persistence
✅ ParameterDiscoveryHelper (read/write/filter params)
✅ MapParametersDialog.xaml (source → target dropdowns)
✅ BatchOperationsCommand with three operations (Clear/Set/Report)
✅ BatchOperationsDialog.xaml (radio buttons + value input)
✅ SpoolManagerCommand (list/rename/delete spools across model)
✅ SpoolManagerDialog.xaml (DataGrid with spool counts)

### Phase 3 (In Progress)
✅ xUnit test project (RevitSpoolCopy.Tests) + RevitSpoolCopy.sln
✅ 12 passing tests (ParameterMappingConfig round-trip, BatchOperation, ParameterMapping)
✅ Revit API via NuGet ref assemblies (CI-friendly, no local Revit to compile)
✅ GitHub Actions CI (.github/workflows/ci.yml: restore/build/test on push+PR)
- [ ] Tests for ParameterDiscoveryHelper / FabricationPartHelper — blocked: these take
      sealed Revit types (Element/FabricationPart/Document) with no interfaces/ctors.
      Needs a refactor to extract pure logic, or Revit-runtime integration tests.
- [ ] Release notes and version tagging
- [ ] Revit App Store publishing (optional)

