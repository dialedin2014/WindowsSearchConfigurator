# Quickstart Guide: Windows Search Configurator

Get up and running with Windows Search Configurator in minutes.

---

## Prerequisites

- **Operating System**: Windows 10, Windows 11, or Windows Server 2016+
- **.NET Runtime**: .NET 8.0 or later
- **Privileges**: Administrator access for modifying index rules
- **Service**: Windows Search service must be running

---

## Installation

### Option 1: Binary Release (Recommended)

1. Download the latest release from the [releases page](https://github.com/yourusername/WindowsSearchConfigurator/releases)
2. Extract to a folder (e.g., `C:\Program Files\WindowsSearchConfigurator`)
3. Add the folder to your system PATH (optional, for convenience)

### Option 2: Build from Source

```powershell
# Clone the repository
git clone https://github.com/yourusername/WindowsSearchConfigurator.git
cd WindowsSearchConfigurator

# Build the project
dotnet build --configuration Release

# Publish as self-contained executable
dotnet publish -c Release -r win-x64 --self-contained
```

The executable will be in `bin/Release/net8.0/win-x64/publish/`

---

## Verify Installation

```powershell
# Check version
WindowsSearchConfigurator.exe --version

# Display help
WindowsSearchConfigurator.exe --help
```

Expected output:
```
Windows Search Configurator v1.0.0
Copyright (c) 2025
Target: Windows 10/11, Windows Server 2016+
.NET Runtime: 8.0.0
```

---

## Quick Start: Common Tasks

### 1. View Current Index Rules

```powershell
# List all configured rules
WindowsSearchConfigurator.exe list
```

**Sample Output:**
```
┌─────────────────────┬─────────┬───────────┬─────────┬────────┐
│ Path                │ Type    │ Recursive │ Filters │ Source │
├─────────────────────┼─────────┼───────────┼─────────┼────────┤
│ D:\Projects         │ Include │ Yes       │ 3       │ User   │
│ D:\Downloads        │ Include │ No        │ 0       │ User   │
│ C:\Temp             │ Exclude │ Yes       │ 0       │ User   │
└─────────────────────┴─────────┴───────────┴─────────┴────────┘
```

### 2. Add a Folder to Search Index

**Simple Add (Recursive):**
```powershell
# Run as Administrator
WindowsSearchConfigurator.exe add "D:\Projects"
```

**Add with File Type Filters:**
```powershell
# Index only C# project files
WindowsSearchConfigurator.exe add "D:\Development" --include *.cs,*.csproj,*.sln
```

**Add with Exclusions:**
```powershell
# Index but exclude build artifacts
WindowsSearchConfigurator.exe add "D:\Projects" --exclude-folders bin,obj,node_modules
```

**Add Non-Recursive (Single Folder Only):**
```powershell
# Index only top-level files
WindowsSearchConfigurator.exe add "D:\Logs" --non-recursive
```

### 3. Remove a Folder from Index

```powershell
# Remove with confirmation prompt
WindowsSearchConfigurator.exe remove "D:\Projects"

# Remove without confirmation (for scripts)
WindowsSearchConfigurator.exe remove "D:\Projects" --no-confirm
```

### 4. Modify an Existing Rule

```powershell
# Change to non-recursive
WindowsSearchConfigurator.exe modify "D:\Projects" --recursive false

# Update file type filters
WindowsSearchConfigurator.exe modify "D:\Projects" --include *.cs,*.xaml --exclude-files *.g.cs
```

### 5. Backup and Restore Configuration

**Export Configuration:**
```powershell
# Export current rules to JSON
WindowsSearchConfigurator.exe export "backup-2025-10-22.json"

# Export including Windows defaults
WindowsSearchConfigurator.exe export "full-config.json" --include-defaults
```

**Import Configuration:**
```powershell
# Restore from backup (replaces current rules)
WindowsSearchConfigurator.exe import "backup-2025-10-22.json"

# Merge with existing rules
WindowsSearchConfigurator.exe import "additional-rules.json" --merge

# Test import without applying changes
WindowsSearchConfigurator.exe import "config.json" --dry-run
```

### 6. Manage File Extension Indexing

**Search for Extensions:**
```powershell
# List all file extensions
WindowsSearchConfigurator.exe search-extensions

# Find log file extensions
WindowsSearchConfigurator.exe search-extensions "*.log"

# Find extensions with full-content indexing
WindowsSearchConfigurator.exe search-extensions --depth properties-and-contents
```

**Configure Extension Indexing Depth:**
```powershell
# Index only properties (metadata) for log files
WindowsSearchConfigurator.exe configure-depth .log properties-only

# Index full content for C# files
WindowsSearchConfigurator.exe configure-depth .cs properties-and-contents
```

---

## Real-World Workflows

### Workflow 1: Developer Workstation Setup

Configure Windows Search for optimal code search:

```powershell
# Add development folder with relevant file types
WindowsSearchConfigurator.exe add "D:\Development" `
  --include *.cs,*.csproj,*.sln,*.md,*.json,*.xml,*.xaml `
  --exclude-folders bin,obj,node_modules,.vs,.git,packages

# Configure source files for full-content indexing
WindowsSearchConfigurator.exe configure-depth .cs properties-and-contents
WindowsSearchConfigurator.exe configure-depth .md properties-and-contents

# Export configuration for team sharing
WindowsSearchConfigurator.exe export "dev-search-config.json"
```

### Workflow 2: Exclude Temporary Folders

Prevent Windows Search from indexing temporary/cache folders:

```powershell
# Add exclusion rules
WindowsSearchConfigurator.exe add "C:\Temp" --type exclude
WindowsSearchConfigurator.exe add "C:\Windows\Temp" --type exclude
WindowsSearchConfigurator.exe add "$env:LOCALAPPDATA\Temp" --type exclude
```

### Workflow 3: Network Share Indexing

Configure indexing for a network share:

```powershell
# Add UNC path with document filters
WindowsSearchConfigurator.exe add "\\fileserver\documents" `
  --include *.docx,*.xlsx,*.pdf,*.txt `
  --exclude-folders Archive,Backup

# Configure extensions for properties-only (faster on network)
WindowsSearchConfigurator.exe configure-depth .pdf properties-only
```

### Workflow 4: Log File Management

Index log locations with metadata-only indexing:

```powershell
# Add log directory non-recursively
WindowsSearchConfigurator.exe add "D:\Logs" --non-recursive --include *.log,*.txt

# Configure log extensions for properties-only
WindowsSearchConfigurator.exe configure-depth .log properties-only
```

### Workflow 5: Configuration Migration

Move configuration between machines:

```powershell
# On source machine
WindowsSearchConfigurator.exe export "my-config.json" --include-extensions

# Transfer file to target machine, then:
# On target machine (as Administrator)
WindowsSearchConfigurator.exe import "my-config.json"
```

---

## Troubleshooting

### Issue: "Operation requires administrator privileges"

**Cause:** Modifying index rules requires elevated permissions.

**Solution:**
```powershell
# Right-click PowerShell/Terminal and select "Run as Administrator"
# Then run your command
WindowsSearchConfigurator.exe add "D:\Projects"
```

### Issue: "Windows Search service is not running"

**Cause:** The Windows Search service (wsearch) is stopped or disabled.

**Solution:**
```powershell
# Start the service
net start wsearch

# Or use Services.msc GUI:
# 1. Press Win+R, type "services.msc"
# 2. Find "Windows Search"
# 3. Right-click → Start
```

### Issue: "Path is invalid: directory does not exist"

**Cause:** The specified path doesn't exist or is inaccessible.

**Solution:**
```powershell
# Verify path exists
Test-Path "D:\Projects"

# Check for typos in path
# Ensure network paths are accessible
```

### Issue: Import fails with "Configuration file has invalid format"

**Cause:** JSON file is malformed or doesn't match expected schema.

**Solution:**
```powershell
# Validate JSON syntax using online validator or:
Get-Content "config.json" | ConvertFrom-Json

# Compare against schema in docs/contracts/configuration-schema.json
# Ensure version field is present and valid
```

### Issue: Changes not reflected in Windows Search

**Cause:** Windows Search indexer may need time to process changes or requires rebuild.

**Solution:**
```powershell
# Wait 5-10 minutes for indexer to process changes
# Force rebuild index (if needed):
# 1. Open Indexing Options (Control Panel)
# 2. Click "Advanced" → "Rebuild"

# Check indexing status
# Control Panel → Indexing Options → Check status message
```

### Issue: "Path exceeds maximum length"

**Cause:** Path is longer than 260 characters (Windows MAX_PATH limit).

**Solution:**
- Use shorter folder names
- Move folders closer to drive root
- Enable long path support in Windows (requires registry edit and reboot)

---

## Getting Help

### Built-in Help

```powershell
# General help
WindowsSearchConfigurator.exe --help

# Command-specific help
WindowsSearchConfigurator.exe add --help
WindowsSearchConfigurator.exe export --help
```

### Verbose Logging

Enable detailed logging for troubleshooting:

```powershell
WindowsSearchConfigurator.exe add "D:\Projects" --verbose
```

### Audit Logs

Review audit logs for operation history:

```powershell
# Default location: %APPDATA%\WindowsSearchConfigurator\audit.log
notepad "$env:APPDATA\WindowsSearchConfigurator\audit.log"
```

### Additional Resources

- **Full Documentation**: See `docs/` folder
- **Issue Tracker**: [GitHub Issues](https://github.com/yourusername/WindowsSearchConfigurator/issues)
- **API Reference**: `docs/contracts/cli-contract.md`
- **JSON Schema**: `docs/contracts/configuration-schema.json`

---

## Tips and Best Practices

### Performance

✅ **DO**:
- Use file type filters to limit indexing scope
- Configure large binary files as properties-only
- Exclude build artifacts (bin, obj, node_modules)
- Use non-recursive for log directories with many files

❌ **DON'T**:
- Index entire system drives unnecessarily
- Index very large files without filters
- Add overlapping or conflicting rules

### Security

✅ **DO**:
- Run as Administrator only when required
- Review imported configurations before applying
- Keep audit logs enabled
- Export configuration backups regularly

❌ **DON'T**:
- Run untrusted configuration files
- Disable confirmation prompts in interactive sessions
- Index directories with sensitive data unless necessary

### Maintenance

✅ **DO**:
- Review index rules periodically
- Remove rules for deleted/unmapped folders
- Update extension settings as needed
- Document custom configurations

❌ **DON'T**:
- Accumulate obsolete rules
- Ignore Windows Search service health
- Skip backing up configurations before bulk changes

---

## Next Steps

- **Explore Advanced Features**: See `docs/user-guide.md` for detailed information
- **Automate Configuration**: Use PowerShell scripts with `--no-confirm` flag
- **Team Sharing**: Export/import configurations across development teams
- **CI/CD Integration**: Include configuration management in deployment scripts

---

## Quick Reference Card

| Task | Command |
|------|---------|
| List rules | `list` |
| Add folder (recursive) | `add "path"` |
| Add with filters | `add "path" --include *.ext` |
| Add non-recursive | `add "path" --non-recursive` |
| Remove folder | `remove "path"` |
| Modify rule | `modify "path" --recursive false` |
| Export config | `export "file.json"` |
| Import config | `import "file.json"` |
| Search extensions | `search-extensions "*.log"` |
| Configure extension | `configure-depth .txt properties-only` |
| Get help | `--help` or `<command> --help` |

---

**Need more help?** Check the full documentation or open an issue on GitHub.
