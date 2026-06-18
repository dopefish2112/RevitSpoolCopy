# RevitSpoolCopy Specification

## Purpose
Revit addin (Revit 2025-2027) that copies the "Assembly Name" parameter value from selected MEP fabrication parts into their Spool field.

## Scope
- **Target:** Autodesk Revit 2025, 2026, 2027 (all use .NET 8)
- **Language:** C# .NET 8, x64 only
- **Dependencies:** RevitAPI.dll, RevitAPIUI.dll (from Revit install)

## Feature: Copy Assembly Name → Spool
1. User selects one or more MEP fabrication parts in the model
2. User clicks ribbon button "Assembly Name → Spool"
3. For each selected part:
   - Read "Assembly Name" (builtin ASSEMBLY_NAME param, fallback to custom param)
   - Write to native `SpoolName` field
4. Dialog shows: # copied, # skipped (no source value)

## Deployment
- Single x64 DLL works for Revit 2025, 2026, 2027
- `deploy.ps1` builds against any installed Revit, installs manifest to `%APPDATA%\Autodesk\Revit\Addins\{version}`
- Manifest points at `bin\Release\RevitSpoolCopy.dll`

## Future Architecture
- Designed for expansion into pluginbundle model
- Modular command structure allows adding commands without refactoring core ribbon
- Consider: parameter mapping config, batch operations, logging

## License
MIT
