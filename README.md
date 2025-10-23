# Windows Search Configurator

A command-line tool for managing Windows Search index rules and file extension indexing settings. Provides full control over which folders and file types are indexed by Windows Search.

## Features

- ✅ **View Index Rules** - List all configured index rules with multiple output formats (table, JSON, CSV)
- ✅ **Add Index Rules** - Add folders to Windows Search index with file type filters and exclusion patterns
- ✅ **Remove Index Rules** - Remove folders from index with confirmation prompts
- ✅ **Modify Index Rules** - Update existing rules without removing and re-adding
- ✅ **Search Extensions** - Find file extensions and their indexing depth settings with wildcard support
- ✅ **Configure Depth** - Set indexing depth for file extensions (Properties only vs Properties and Contents)
- ✅ **Import/Export** - Backup and restore configuration in JSON format with merge and validation options

## Requirements

- **Operating System**: Windows 10, Windows 11, or Windows Server 2016+
- **.NET Runtime**: .NET 8.0 or later
- **Privileges**: 
  - Standard user: Read-only operations (list, search-extensions, export)
  - Administrator: All modification operations (add, remove, modify, configure-depth, import)
- **Service**: Windows Search service (`WSearch`) must be running

## Installation

### Option 1: Download Release

1. Download the latest release from the [releases page](https://github.com/yourusername/WindowsSearchConfigurator/releases)
2. Extract to a folder (e.g., `C:\Program Files\WindowsSearchConfigurator`)
3. Optionally add the folder to your system PATH

### Option 2: Build from Source

```powershell
# Clone repository
git clone https://github.com/yourusername/WindowsSearchConfigurator.git
cd WindowsSearchConfigurator

# Build
dotnet build --configuration Release

# Publish self-contained
dotnet publish -c Release -r win-x64 --self-contained
```

The executable will be in `src/WindowsSearchConfigurator/bin/Release/net8.0/win-x64/publish/`

## Quick Start

### View Current Rules

```powershell
# List all rules in table format
WindowsSearchConfigurator.exe list

# List rules in JSON format
WindowsSearchConfigurator.exe list --format json

# List rules including Windows defaults
WindowsSearchConfigurator.exe list --show-defaults

# Filter rules by path pattern
WindowsSearchConfigurator.exe list --filter "D:\Projects\*"
```

### Add a Folder to Index

```powershell
# Add folder (requires admin)
WindowsSearchConfigurator.exe add "D:\Projects"

# Add with file type filters
WindowsSearchConfigurator.exe add "D:\Development" --include *.cs,*.csproj,*.sln

# Add with exclusions
WindowsSearchConfigurator.exe add "D:\Projects" --exclude-folders bin,obj,node_modules

# Add non-recursive (single folder only)
WindowsSearchConfigurator.exe add "D:\Logs" --non-recursive
```

### Remove a Folder from Index

```powershell
# Remove with confirmation
WindowsSearchConfigurator.exe remove "D:\Projects"

# Remove without confirmation (scripting)
WindowsSearchConfigurator.exe remove "D:\Projects" --force
```

### Modify Existing Rules

```powershell
# Change to non-recursive
WindowsSearchConfigurator.exe modify "D:\Projects" --recursive false

# Update file filters
WindowsSearchConfigurator.exe modify "D:\Projects" --include *.cs,*.xaml

# Change rule type
WindowsSearchConfigurator.exe modify "D:\Temp" --type exclude
```

### Manage File Extensions

```powershell
# Search all extensions
WindowsSearchConfigurator.exe search-extensions

# Find log file extensions
WindowsSearchConfigurator.exe search-extensions "*.log"

# Find extensions with full-content indexing
WindowsSearchConfigurator.exe search-extensions --depth PropertiesAndContents

# Configure extension depth (requires admin)
WindowsSearchConfigurator.exe configure-depth .log PropertiesOnly
WindowsSearchConfigurator.exe configure-depth .cs PropertiesAndContents
```

### Import/Export Configuration

```powershell
# Export current configuration
WindowsSearchConfigurator.exe export "backup.json"

# Export including Windows defaults
WindowsSearchConfigurator.exe export "full-config.json" --include-defaults

# Validate before import (dry-run)
WindowsSearchConfigurator.exe import "backup.json" --dry-run

# Import configuration (requires admin)
WindowsSearchConfigurator.exe import "backup.json"

# Merge with existing rules
WindowsSearchConfigurator.exe import "additional-rules.json" --merge

# Continue on errors
WindowsSearchConfigurator.exe import "config.json" --continue-on-error
```

## Command Reference

### Global Options

- `--help`, `-h` - Display help information
- `--version` - Display version information

### `list` Command

List all configured Windows Search index rules.

**Options:**
- `--format`, `-f` - Output format: `table` (default), `json`, or `csv`
- `--show-defaults`, `-d` - Include Windows system default rules
- `--filter`, `-p` - Filter rules by path pattern (supports wildcards)

### `add` Command

Add a location to the Windows Search index. **Requires administrator privileges.**

**Arguments:**
- `path` - Path to add to the index (local or UNC path)

**Options:**
- `--non-recursive`, `-nr` - Index only the specified folder, not subfolders
- `--include`, `-i` - File type patterns to include (e.g., `*.cs,*.txt`)
- `--exclude-files`, `-ef` - File name patterns to exclude (e.g., `*.tmp,*.log`)
- `--exclude-folders`, `-ed` - Subfolder names to exclude (e.g., `bin,obj,node_modules`)
- `--type`, `-t` - Rule type: `Include` (default) or `Exclude`

### `remove` Command

Remove a location from the Windows Search index. **Requires administrator privileges.**

**Arguments:**
- `path` - Path to remove from the index

**Options:**
- `--force`, `-f`, `--no-confirm` - Skip confirmation prompt

### `modify` Command

Modify an existing Windows Search index rule. **Requires administrator privileges.**

**Arguments:**
- `path` - Path of the rule to modify

**Options:**
- `--recursive`, `-r` - Set whether to index subfolders (true or false)
- `--include`, `-i` - File type patterns to include
- `--exclude-files`, `-ef` - File name patterns to exclude
- `--exclude-folders`, `-ed` - Subfolder names to exclude
- `--type`, `-t` - Rule type: `Include` or `Exclude`
- `--force`, `-f`, `--no-confirm` - Skip confirmation prompt

### `search-extensions` Command

Search for file extensions and their indexing settings.

**Arguments:**
- `pattern` - Wildcard pattern to match extensions (default: `*`)

**Options:**
- `--format`, `-f` - Output format: `table` (default), `json`, or `csv`
- `--depth`, `-d` - Filter by indexing depth: `NotIndexed`, `PropertiesOnly`, or `PropertiesAndContents`

### `configure-depth` Command

Configure indexing depth for a file extension. **Requires administrator privileges.**

**Arguments:**
- `extension` - File extension (e.g., `.txt`, `.log`)
- `depth` - Indexing depth: `NotIndexed`, `PropertiesOnly`, or `PropertiesAndContents`

### `export` Command

Export current configuration to JSON file.

**Arguments:**
- `file` - Path to the output JSON file

**Options:**
- `--include-defaults`, `-d` - Include Windows system default rules
- `--include-extensions`, `-e` - Include file extension settings (default: true)
- `--overwrite`, `-o` - Overwrite existing file without prompting

### `import` Command

Import configuration from JSON file. **Requires administrator privileges** (unless using `--dry-run`).

**Arguments:**
- `file` - Path to the input JSON file

**Options:**
- `--merge`, `-m` - Merge with existing rules instead of replacing
- `--continue-on-error`, `-c` - Continue importing even if individual rules fail
- `--dry-run`, `-n` - Validate configuration without applying changes

## Exit Codes

- `0` - Success
- `1` - General error
- `2` - Operation failed
- `3` - Invalid input
- `4` - Insufficient privileges (administrator required)
- `5` - Service unavailable (Windows Search not running)

## Real-World Scenarios

### Developer Workstation Setup

Configure Windows Search for optimal code search:

```powershell
# Add development folder with relevant file types
WindowsSearchConfigurator.exe add "D:\Development" `
  --include *.cs,*.csproj,*.sln,*.md,*.json,*.xml,*.xaml `
  --exclude-folders bin,obj,node_modules,.vs,.git,packages

# Configure source files for full-content indexing
WindowsSearchConfigurator.exe configure-depth .cs PropertiesAndContents
WindowsSearchConfigurator.exe configure-depth .md PropertiesAndContents

# Export configuration for team sharing
WindowsSearchConfigurator.exe export "dev-search-config.json"
```

### Exclude Temporary Folders

Prevent indexing of temporary and cache folders:

```powershell
WindowsSearchConfigurator.exe add "C:\Temp" --type Exclude
WindowsSearchConfigurator.exe add "C:\Windows\Temp" --type Exclude
WindowsSearchConfigurator.exe add "%LOCALAPPDATA%\Temp" --type Exclude
```

### Server Deployment

Standardize search configuration across multiple servers:

```powershell
# On reference server
WindowsSearchConfigurator.exe export "server-config.json"

# On target servers (automated deployment)
WindowsSearchConfigurator.exe import "server-config.json" --continue-on-error
```

## Configuration File Format

The export/import feature uses JSON format:

```json
{
  "version": "1.0",
  "exportDate": "2025-10-22T10:30:00Z",
  "exportedBy": "DOMAIN\\Username",
  "machineName": "WORKSTATION01",
  "rules": [
    {
      "path": "D:\\Projects",
      "ruleType": "Include",
      "recursive": true,
      "fileTypeFilters": [
        {
          "pattern": "*.cs",
          "filterType": "Include",
          "appliesTo": "FileExtension"
        }
      ],
      "excludedSubfolders": ["bin", "obj"],
      "source": "User"
    }
  ],
  "extensionSettings": [
    {
      "extension": ".log",
      "indexingDepth": "PropertiesOnly",
      "isDefaultSetting": false
    }
  ]
}
```

## Troubleshooting

### "Microsoft Windows Search COM API is not registered"

The tool requires the Windows Search COM API to be registered. This error appears when the API is not properly registered on your system.

**Automatic Registration (Recommended)**:

```powershell
# Interactive mode - prompts for confirmation
WindowsSearchConfigurator.exe list

# Automatic mode - no prompt (requires admin)
WindowsSearchConfigurator.exe --auto-register-com list
```

**Manual Registration**:

1. Open Command Prompt as Administrator
2. Run the following command:

```powershell
regsvr32 "%SystemRoot%\System32\SearchAPI.dll"
```

3. Restart WindowsSearchConfigurator

**CI/CD Environments**:

Use the `--auto-register-com` flag to automatically register without prompts, or `--no-register-com` to fail immediately if not registered:

```powershell
# Auto-register if needed (requires admin)
WindowsSearchConfigurator.exe --auto-register-com export --output config.json

# Fail-fast if not registered (pre-check)
WindowsSearchConfigurator.exe --no-register-com list
```

**Common Causes**:
- Windows Search not installed or disabled
- SearchAPI.dll missing or corrupted
- Registry keys missing or damaged
- Recent Windows updates that affected Windows Search

**Resolution Steps**:
1. Verify Windows Search is installed: `Get-Service WSearch`
2. Check if SearchAPI.dll exists: `Test-Path "$env:SystemRoot\System32\SearchAPI.dll"`
3. Try manual registration (above)
4. If problem persists, repair or reinstall Windows Search

### "Windows Search service is not running"

Start the Windows Search service:

```powershell
Start-Service WSearch
```

### "Access denied. Administrator privileges required"

Right-click Command Prompt or PowerShell and select "Run as administrator".

### Changes Not Reflected Immediately

Windows Search may take time to re-index. Force a rebuild:

```powershell
# Stop and restart the service
Stop-Service WSearch
Start-Service WSearch

# Or use the Indexing Options control panel:
# Control Panel → Indexing Options → Advanced → Rebuild
```

### Path Too Long (MAX_PATH)

Windows Search has a 260-character path limit. Use shorter paths or enable long path support in Windows 10 1607+:

```powershell
# Enable long paths (requires admin and reboot)
New-ItemProperty -Path "HKLM:\SYSTEM\CurrentControlSet\Control\FileSystem" `
  -Name "LongPathsEnabled" -Value 1 -PropertyType DWORD -Force
```

## Architecture

- **Language**: C# / .NET 8.0 (LTS)
- **Windows APIs**: COM (ISearchCrawlScopeManager), Registry, WMI
- **CLI Framework**: System.CommandLine
- **JSON**: System.Text.Json
- **Testing**: NUnit, Moq, FluentAssertions

## Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## License

See [LICENSE](LICENSE) file for details.

## Acknowledgments

- Built following the [SpecKit](https://github.com/dialedin/speckit) development methodology
- Uses Microsoft's Windows Search APIs and .NET framework

## Support

For issues, questions, or feature requests, please [open an issue](https://github.com/yourusername/WindowsSearchConfigurator/issues).
