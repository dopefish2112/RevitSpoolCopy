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
│   ├── MapParametersCommand.cs (stub: coming Phase 2)
│   └── BatchOperationsCommand.cs (stub: coming Phase 2)
├── Models/
│   └── FabricationPartHelper.cs (parameter read/write logic)
├── UI/
│   └── [dialogs, forms - Phase 2+]
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
```powershell
# Build for Revit 2025 (default)
dotnet build -c Release

# Build for Revit 2026
dotnet build -c Release -p:RevitDir="C:\Program Files\Autodesk\Revit 2026"

# Install for all Revit versions
.\deploy.ps1

# Install for specific version
.\deploy.ps1 -Versions 2026
```

Output: `bin\Release\RevitSpoolCopy.dll` (x64, single DLL for 2025/2026/2027)

### Phase 1 (Complete)
✅ Refactored to modular, extensible architecture
✅ Created ICommand interface
✅ Extracted FabricationPartHelper utilities
✅ Created command stubs (MapParametersCommand, BatchOperationsCommand)
✅ Single CommandRouter entry point
✅ Build succeeds, single DLL works across Revit versions

### Phase 2 (Next)
- [ ] MapParametersCommand with UI dialog
- [ ] ParameterMapping model (source param → target param)
- [ ] Parameter mapping storage (JSON config file)
- [ ] BatchOperationsCommand with operation selection
- [ ] Progress dialog for large selections

### Phase 3 (Future)
- [ ] SpoolManager command (list/edit all spools in model)
- [ ] Unit tests
- [ ] GitHub Actions CI/CD
- [ ] Release notes and installer
- [ ] Revit App Store publishing (maybe)

## Next Steps
1. Commit refactoring to main branch
2. Create dev branch for Phase 2 features
3. Implement MapParametersCommand with UI

