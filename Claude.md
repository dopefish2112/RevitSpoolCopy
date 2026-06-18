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
✅ Revit API via NuGet ref assemblies (CI-friendly, no local Revit to compile)
✅ GitHub Actions CI (.github/workflows/ci.yml: restore/build/test on push+PR)
✅ Testability seam: IElementView/IParameterView + pure ParameterLogic; Revit helpers
   (FabricationPartHelper/ParameterDiscoveryHelper) are thin adapters over RevitElementView
✅ 33 passing tests (config round-trip, BatchOperation, ParameterMapping, ParameterLogic
   read/write/discovery + Assembly Name special-casing, via in-memory fakes)
- [ ] Release notes and version tagging
- [ ] Revit App Store publishing (optional)

### Phase 4 (Implemented, pending Revit runtime test) — Spool-driven export & publishing
✅ MAJ export confirmed in public API: `FabricationPart.SaveAsFabricationJob(doc, ISet<ElementId>,
   filename, FabricationSaveJobOptions(bool))` (Autodesk.Revit.DB.Fabrication). Needs an open
   transaction + a loaded Fabrication Configuration.
✅ Batch Ops dialog extended: model-wide spool checklist + two new operations.
✅ Op "Export selected spools to MAJ" — one .MAJ per spool into a chosen folder
   (FabricationJobExporter + Microsoft.Win32.OpenFolderDialog).
✅ Op "Create publish set + isolated view per spool" — View3D.CreateIsometric +
   IsolateElementsTemporary→permanent, gathered into a ViewSheetSet (SpoolViewPublisher).
✅ Pure SpoolExportLogic (spool→part-id filter, MAJ filename sanitize, view naming) + 16 tests.
   Runtime-untested in Revit: MAJ config dependency, PrintManager publish-set save path.

#### Original plan (for reference)
Extend **Batch Operations** to act on a selected group of spools (multi-select from the
spool list, like SpoolManager's grouping). Two new operations:

1. **Export spools to MAJ (Fabrication job export)**
   - Gather all Fabrication MEP parts whose Spool designation matches the selected spools.
   - Invoke the **MAJ fabrication export** — the export function that ships with the Revit
     **Fabrication extension** (Autodesk Fabrication `.MAJ` job file). Need to confirm the
     extension's public API/entry point and whether it's callable from an add-in or only
     via its own UI; may require referencing the extension assembly or shelling out.
   - Open question: one MAJ per spool, or one MAJ for the whole selection? (Likely per-spool
     to match spool designations.)

2. **Create publish set + isolated view per spool selection**
   - Generate a new view containing *only* the parts in the selected spools (everything else
     hidden/isolated — probably a 3D view with a filter or temporary isolate baked in).
   - Add that view to a **new publish set** (Sheet/View set used for publishing/exporting).
   - The view will likely have to be generated programmatically (duplicate a template 3D
     view, apply a spool-based filter, name it after the spool).

Implementation notes / unknowns to resolve before building:
- MAJ export: locate the Fabrication-extension API surface (assembly name, namespace,
  the export method). This is the riskiest unknown — confirm it's automatable.
- "Publish set": confirm whether this means a Revit **View/Sheet Set** (ViewSheetSet) or an
  export-specific publish set. Implement via `ViewSheetSet` + `PrintManager`/export if the
  former.
- View generation: `View3D.CreateIsometric` + `ParameterFilterElement` keyed on the Spool
  shared parameter (added in Phase 3) to show only matching parts.
- UI: BatchOperationsDialog needs a spool multi-select (reuse SpoolManager's spool listing).

### Testability pattern
Revit types (Element/FabricationPart/Parameter) are sealed with no public ctors, so they
can't be mocked directly. Decision logic lives in `ParameterLogic` over the Revit-free
`IElementView`/`IParameterView` seam; `RevitElementView`/`RevitParameterView` adapt the
real Revit objects. Tests drive the logic with `FakeElement`/`FakeParameter`. Add new
parameter logic to `ParameterLogic` (testable), not to the Revit adapters.

