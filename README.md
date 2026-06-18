# RevitSpoolCopy

Autodesk Revit addin (2025-2027) that copies "Assembly Name" parameter values to the Spool field on selected MEP fabrication parts.

## Quick Start

### Build
```powershell
dotnet build RevitSpoolCopy.csproj -c Release
# Or override Revit version:
dotnet build -c Release -p:RevitDir="C:\Program Files\Autodesk\Revit 2026"
```

### Install
```powershell
.\deploy.ps1                # All installed Revit versions
.\deploy.ps1 -Versions 2026 # Specific version
```
Then restart Revit. Button appears in **Spool Tools** ribbon tab.

## Usage
1. Select one or more fabrication parts in model
2. Click **Assembly Name → Spool** button
3. Dialog confirms # copied & # skipped

## Documentation
- [spec.md](spec.md) — Feature spec & architecture
- [Claude.md](Claude.md) — Dev notes & build walkthrough

## License
MIT

## Author
Mike Chandler
