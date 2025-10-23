# CLI Contract: Windows Search Configurator

**Version**: 1.0  
**Date**: 2025-10-22

## Overview

This document defines the command-line interface contract for the Windows Search Configurator. The CLI follows standard conventions and provides a consistent, predictable interface for all operations.

---

## Global Syntax

```
WindowsSearchConfigurator.exe <command> [arguments] [options]
```

### Global Options

Available for all commands:

| Option | Alias | Type | Description |
|--------|-------|------|-------------|
| `--help` | `-h`, `-?` | Flag | Display help information for the command |
| `--version` | | Flag | Display application version |
| `--no-confirm` | | Flag | Skip confirmation prompts (for automation) |
| `--verbose` | `-v` | Flag | Enable verbose output with detailed logging |

### Exit Codes

| Code | Meaning |
|------|---------|
| 0 | Success |
| 1 | General error |
| 2 | Invalid arguments or options |
| 3 | Permission denied (admin required) |
| 4 | Windows Search service unavailable |
| 5 | Configuration file error |

---

## Commands

### 1. list

Lists all configured Windows Search index rules.

**Syntax:**
```
WindowsSearchConfigurator.exe list [options]
```

**Options:**

| Option | Type | Default | Description |
|--------|------|---------|-------------|
| `--format` | String | table | Output format: table, json, csv |
| `--show-defaults` | Flag | false | Include Windows default rules |
| `--filter` | String | | Filter by path pattern (wildcards supported) |

**Output Format (table):**
```
ID                                   Path              Type    Recursive  Filters  Source
------------------------------------ ----------------- ------- ---------- -------- ------
550e8400-e29b-41d4-a716-446655440000 D:\Projects       Include Yes        2        User
6ba7b810-9dad-11d1-80b4-00c04fd430c8 C:\Temp           Exclude No         0        User
```

**Examples:**
```powershell
# List all user-configured rules
WindowsSearchConfigurator.exe list

# List all rules including Windows defaults
WindowsSearchConfigurator.exe list --show-defaults

# List rules as JSON
WindowsSearchConfigurator.exe list --format json

# Filter rules by path
WindowsSearchConfigurator.exe list --filter "D:\*"
```

---

### 2. add

Adds a new location to the Windows Search index.

**Syntax:**
```
WindowsSearchConfigurator.exe add <path> [options]
```

**Arguments:**

| Argument | Required | Description |
|----------|----------|-------------|
| `path` | Yes | Folder path to add to index (local or UNC) |

**Options:**

| Option | Type | Default | Description |
|--------|------|---------|-------------|
| `--non-recursive` | Flag | false | Index only specified folder, not subfolders |
| `--include` | String[] | | File patterns to include (e.g., *.cs,*.csproj) |
| `--exclude-files` | String[] | | File patterns to exclude |
| `--exclude-folders` | String[] | | Subfolder patterns to exclude |
| `--type` | String | include | Rule type: include or exclude |

**Examples:**
```powershell
# Add folder with default recursive indexing
WindowsSearchConfigurator.exe add "D:\Projects"

# Add folder with specific file type filters
WindowsSearchConfigurator.exe add "D:\Projects" --include *.cs,*.csproj,*.sln

# Add folder excluding specific subfolders
WindowsSearchConfigurator.exe add "D:\Projects" --exclude-folders bin,obj,node_modules

# Add non-recursive (single folder only)
WindowsSearchConfigurator.exe add "D:\Logs" --non-recursive

# Add UNC path
WindowsSearchConfigurator.exe add "\\server\share\documents"

# Exclude a folder from indexing
WindowsSearchConfigurator.exe add "C:\Temp" --type exclude
```

**Requires**: Administrator privileges

---

### 3. remove

Removes a location from the Windows Search index.

**Syntax:**
```
WindowsSearchConfigurator.exe remove <path> [options]
```

**Arguments:**

| Argument | Required | Description |
|----------|----------|-------------|
| `path` | Yes | Folder path to remove from index |

**Options:**

| Option | Type | Default | Description |
|--------|------|---------|-------------|
| `--force` | Flag | false | Skip confirmation prompt |

**Examples:**
```powershell
# Remove with confirmation prompt
WindowsSearchConfigurator.exe remove "D:\Projects"

# Remove without confirmation (automation)
WindowsSearchConfigurator.exe remove "D:\Projects" --force

# Remove using global no-confirm flag
WindowsSearchConfigurator.exe remove "D:\Projects" --no-confirm
```

**Requires**: Administrator privileges

---

### 4. modify

Modifies an existing index rule.

**Syntax:**
```
WindowsSearchConfigurator.exe modify <path> [options]
```

**Arguments:**

| Argument | Required | Description |
|----------|----------|-------------|
| `path` | Yes | Path of the rule to modify |

**Options:**

| Option | Type | Description |
|--------|------|-------------|
| `--recursive` | Bool | Enable/disable recursive indexing (true/false) |
| `--include` | String[] | Set file patterns to include (replaces existing) |
| `--exclude-files` | String[] | Set file patterns to exclude (replaces existing) |
| `--exclude-folders` | String[] | Set subfolder patterns to exclude (replaces existing) |
| `--type` | String | Change rule type: include or exclude |

**Examples:**
```powershell
# Change to non-recursive
WindowsSearchConfigurator.exe modify "D:\Projects" --recursive false

# Update file type filters
WindowsSearchConfigurator.exe modify "D:\Projects" --include *.cs,*.vb --exclude-files *.designer.cs

# Change rule type from include to exclude
WindowsSearchConfigurator.exe modify "C:\Temp" --type exclude
```

**Requires**: Administrator privileges

---

### 5. export

Exports current index rules to a JSON configuration file.

**Syntax:**
```
WindowsSearchConfigurator.exe export <file> [options]
```

**Arguments:**

| Argument | Required | Description |
|----------|----------|-------------|
| `file` | Yes | Output JSON file path |

**Options:**

| Option | Type | Default | Description |
|--------|------|---------|-------------|
| `--include-defaults` | Flag | false | Include Windows default rules |
| `--include-extensions` | Flag | true | Include extension settings |
| `--overwrite` | Flag | false | Overwrite if file exists |

**Examples:**
```powershell
# Export user rules to file
WindowsSearchConfigurator.exe export "backup.json"

# Export all rules including defaults
WindowsSearchConfigurator.exe export "full-config.json" --include-defaults

# Export rules only (no extensions)
WindowsSearchConfigurator.exe export "rules-only.json" --include-extensions false

# Overwrite existing file
WindowsSearchConfigurator.exe export "config.json" --overwrite
```

**Requires**: No elevation (read-only operation)

---

### 6. import

Imports index rules from a JSON configuration file.

**Syntax:**
```
WindowsSearchConfigurator.exe import <file> [options]
```

**Arguments:**

| Argument | Required | Description |
|----------|----------|-------------|
| `file` | Yes | Input JSON file path |

**Options:**

| Option | Type | Default | Description |
|--------|------|---------|-------------|
| `--merge` | Flag | false | Merge with existing rules (default: replace) |
| `--continue-on-error` | Flag | false | Continue if some rules fail to import |
| `--dry-run` | Flag | false | Validate without applying changes |

**Examples:**
```powershell
# Import rules (replaces existing)
WindowsSearchConfigurator.exe import "backup.json"

# Import and merge with existing rules
WindowsSearchConfigurator.exe import "backup.json" --merge

# Validate configuration file
WindowsSearchConfigurator.exe import "backup.json" --dry-run

# Import with error tolerance
WindowsSearchConfigurator.exe import "backup.json" --continue-on-error
```

**Requires**: Administrator privileges

---

### 7. search-extensions

Searches for file extensions using wildcard patterns and displays their indexing settings.

**Syntax:**
```
WindowsSearchConfigurator.exe search-extensions [pattern] [options]
```

**Arguments:**

| Argument | Required | Description |
|----------|----------|-------------|
| `pattern` | No | Wildcard pattern to match extensions (default: *.*) |

**Options:**

| Option | Type | Default | Description |
|--------|------|---------|-------------|
| `--format` | String | table | Output format: table, json, csv |
| `--depth` | String | | Filter by indexing depth: properties-only, properties-and-contents |

**Examples:**
```powershell
# List all extensions
WindowsSearchConfigurator.exe search-extensions

# Search for specific pattern
WindowsSearchConfigurator.exe search-extensions "*.log"

# Search extensions ending in 'x'
WindowsSearchConfigurator.exe search-extensions "*.*x"

# Filter by indexing depth
WindowsSearchConfigurator.exe search-extensions --depth properties-only

# Output as JSON
WindowsSearchConfigurator.exe search-extensions --format json
```

**Requires**: No elevation (read-only operation)

---

### 8. configure-depth

Configures indexing depth for a file extension.

**Syntax:**
```
WindowsSearchConfigurator.exe configure-depth <extension> <depth> [options]
```

**Arguments:**

| Argument | Required | Description |
|----------|----------|-------------|
| `extension` | Yes | File extension (e.g., .txt, .log) |
| `depth` | Yes | Indexing depth: properties-only or properties-and-contents |

**Examples:**
```powershell
# Set extension to index properties only
WindowsSearchConfigurator.exe configure-depth .log properties-only

# Set extension to index full content
WindowsSearchConfigurator.exe configure-depth .cs properties-and-contents

# No confirmation prompt (automation)
WindowsSearchConfigurator.exe configure-depth .txt properties-only --no-confirm
```

**Requires**: Administrator privileges

---

## Error Handling

All commands follow consistent error handling patterns:

### Error Message Format
```
✗ Error: <clear description of what went wrong>
→ <suggested action to resolve the issue>
```

### Example Error Messages

**Permission Denied:**
```
✗ Error: Operation 'add index rule' requires administrator privileges.
→ Right-click the application and select 'Run as administrator'.
```

**Invalid Path:**
```
✗ Error: Path 'D:\NonExistent' is invalid: directory does not exist.
→ Verify the path exists and is accessible.
```

**Service Unavailable:**
```
✗ Error: Windows Search service is not running.
→ Start the Windows Search service using Services.msc or run: net start wsearch
```

**Configuration File Error:**
```
✗ Error: Configuration file 'config.json' has invalid format.
→ Verify the file is valid JSON and matches the schema version 1.0.
```

---

## Output Formatting

### Table Format (Default)

Columns aligned with headers, Unicode box-drawing characters for structure:
```
┌──────────────┬─────────────┬─────────┬───────────┐
│ Extension    │ Depth       │ Default │ Modified  │
├──────────────┼─────────────┼─────────┼───────────┤
│ .txt         │ Full        │ No      │ 2025-10-22│
│ .log         │ Properties  │ No      │ 2025-10-22│
└──────────────┴─────────────┴─────────┴───────────┘
```

### JSON Format

Pretty-printed JSON with 2-space indentation:
```json
{
  "rules": [
    {
      "id": "550e8400-e29b-41d4-a716-446655440000",
      "path": "D:\\Projects",
      "ruleType": "include"
    }
  ]
}
```

### CSV Format

Standard RFC 4180 CSV with headers:
```csv
Extension,IndexingDepth,IsDefault,ModifiedDate
.txt,PropertiesAndContents,false,2025-10-22T10:30:00Z
.log,PropertiesOnly,false,2025-10-22T08:00:00Z
```

---

## Progress Indication

For long-running operations (batch import, large rule sets):

```
Importing rules... [################    ] 80% (40/50 rules)
```

- Use carriage return for same-line updates
- Show completion percentage and item counts
- Display final summary on completion

---

## Confirmation Prompts

For destructive operations (when `--no-confirm` not specified):

```
⚠ Warning: This will remove the index rule for 'D:\Projects'.
   Files in this location will no longer be searchable via Windows Search.

Continue? [y/N]:
```

- Clear warning with consequences explained
- Default to No (safe option)
- Accept y/yes (case-insensitive) to proceed
- Any other input cancels operation

---

## Version Information

```
WindowsSearchConfigurator.exe --version
```

Output:
```
Windows Search Configurator v1.0.0
Copyright (c) 2025
Target: Windows 10/11, Windows Server 2016+
.NET Runtime: 8.0.0
```

---

## Help System

### Global Help
```
WindowsSearchConfigurator.exe --help
```

Shows all available commands with brief descriptions.

### Command Help
```
WindowsSearchConfigurator.exe <command> --help
```

Shows detailed help for specific command including:
- Syntax
- Arguments
- Options
- Examples
- Requirements (admin privileges, etc.)

---

## Backward Compatibility

This contract defines version 1.0 of the CLI interface. Future versions will:
- Maintain backward compatibility for all commands
- Deprecate features with warnings before removal
- Use semantic versioning for CLI contract versions
- Document breaking changes in release notes

---

## Contract Validation

All CLI interface changes must:
1. Update this contract document
2. Add contract tests to verify behavior
3. Maintain backward compatibility or version bump
4. Update help text and examples
5. Document in changelog
