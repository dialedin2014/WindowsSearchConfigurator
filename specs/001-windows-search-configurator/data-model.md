# Data Model: Windows Search Configurator

**Feature**: Windows Search Index Rules Manager  
**Date**: 2025-10-22  
**Phase**: 1 - Design & Contracts

## Overview

This document defines the domain models representing Windows Search index configuration. Models are technology-agnostic representations of business entities, with implementation-specific details abstracted away.

---

## Core Entities

### 1. IndexRule

Represents a configuration entry that specifies whether a location should be indexed by Windows Search.

#### Properties

| Property | Type | Required | Description | Validation |
|----------|------|----------|-------------|------------|
| Id | Guid | Yes | Unique identifier for the rule | Auto-generated |
| Path | string | Yes | Absolute path to the indexed location | Must be valid file system path, max 260 chars |
| RuleType | RuleType enum | Yes | Include or Exclude | Must be valid enum value |
| Recursive | bool | Yes | Whether to index subfolders | Default: true |
| FileTypeFilters | List\<FileTypeFilter\> | No | File patterns to include/exclude | Empty list means all files |
| ExcludedSubfolders | List\<string\> | No | Subfolder patterns to skip | Wildcard patterns supported |
| IsUserDefined | bool | Yes | User-configured vs Windows default | Read-only after creation |
| CreatedDate | DateTime | Yes | When rule was created | Auto-set on creation |
| ModifiedDate | DateTime | Yes | When rule was last modified | Auto-updated on change |
| Source | RuleSource enum | Yes | Where the rule originated | System, User, or Imported |

#### Relationships

- Contains 0 to many **FileTypeFilter** objects
- References an **IndexLocation** (the path being indexed)

#### State Transitions

```
[New] → Create → [Active]
[Active] → Modify → [Active]
[Active] → Disable → [Disabled]
[Disabled] → Enable → [Active]
[Any] → Delete → [Deleted]
```

#### Business Rules

- Path must exist at rule creation time (warning if not, but allowed)
- Cannot have duplicate rules for the same path
- Exclude rules take precedence over Include rules in Windows Search
- Recursive=false means only the specified folder, no subdirectories
- FileTypeFilters only apply to Include rules, not Exclude rules

---

### 2. FileTypeFilter

Defines which file extensions, specific files, or subfolders should be included or excluded within an index rule.

#### Properties

| Property | Type | Required | Description | Validation |
|----------|------|----------|-------------|------------|
| Pattern | string | Yes | Wildcard pattern (e.g., *.txt, *.log) | Must be valid wildcard syntax |
| FilterType | FilterType enum | Yes | Include or Exclude | Must be valid enum value |
| AppliesTo | FilterTarget enum | Yes | FileExtension, FileName, or Subfolder | Must be valid enum value |

#### Examples

```csharp
// Include only .cs and .csproj files
new FileTypeFilter { Pattern = "*.cs", FilterType = Include, AppliesTo = FileExtension }
new FileTypeFilter { Pattern = "*.csproj", FilterType = Include, AppliesTo = FileExtension }

// Exclude node_modules and bin subfolders
new FileTypeFilter { Pattern = "node_modules", FilterType = Exclude, AppliesTo = Subfolder }
new FileTypeFilter { Pattern = "bin", FilterType = Exclude, AppliesTo = Subfolder }

// Exclude specific file
new FileTypeFilter { Pattern = "secrets.txt", FilterType = Exclude, AppliesTo = FileName }
```

#### Business Rules

- Patterns are case-insensitive on Windows
- Wildcards: `*` (zero or more chars), `?` (exactly one char)
- Multiple filters combine with AND logic for Include, OR logic for Exclude
- Subfolder patterns can be simple names or relative paths

---

### 3. IndexLocation

A folder path (local or network) that is subject to indexing rules.

#### Properties

| Property | Type | Required | Description | Validation |
|----------|------|----------|-------------|------------|
| FullPath | string | Yes | Normalized absolute path | Must be valid path format |
| PathType | PathType enum | Yes | Local, UNC, or RelativePath | Determined by path format |
| Exists | bool | Yes | Whether location currently exists | Checked at runtime |
| IsAccessible | bool | Yes | Whether current user can read path | Checked at runtime |
| VolumeLabel | string | No | Drive/volume label if applicable | Only for local paths |

#### Path Type Detection

```csharp
PathType DeterminePathType(string path)
{
    if (path.StartsWith(@"\\")) 
        return PathType.UNC;
    if (Path.IsPathRooted(path))
        return PathType.Local;
    return PathType.Relative;
}
```

#### Business Rules

- All paths stored internally as absolute paths
- Relative paths resolved against current directory at input time
- UNC paths must follow `\\server\share\path` format
- Paths normalized to remove `.` and `..` segments
- Maximum length: 260 characters (MAX_PATH on Windows)

---

### 4. FileExtensionSetting

Represents the indexing depth configuration for a specific file extension.

#### Properties

| Property | Type | Required | Description | Validation |
|----------|------|----------|-------------|------------|
| Extension | string | Yes | File extension (e.g., .txt, .log) | Must start with dot |
| IndexingDepth | IndexingDepth enum | Yes | Properties only or Properties and Contents | Must be valid enum value |
| IsDefaultSetting | bool | Yes | Windows default vs user-configured | Read-only |
| ModifiedDate | DateTime | Yes | When setting was last changed | Auto-updated on change |

#### Indexing Depth Values

- **PropertiesOnly**: Index file metadata (name, size, dates, attributes) but not contents
- **PropertiesAndContents**: Index both metadata and full-text content of files

#### Business Rules

- Extension must include leading dot (`.txt` not `txt`)
- Extensions are case-insensitive
- Default settings cannot be modified (read-only)
- User can override defaults by setting custom values
- Some extensions (e.g., `.exe`, `.dll`) are typically PropertiesOnly for security

---

### 5. ConfigurationFile

A portable JSON representation of multiple index rules and extension settings that can be exported/imported.

#### Properties

| Property | Type | Required | Description | Validation |
|----------|------|----------|-------------|------------|
| Version | string | Yes | Schema version (e.g., "1.0") | Semantic versioning |
| ExportDate | DateTime | Yes | When configuration was exported | ISO 8601 format |
| ExportedBy | string | Yes | User who exported (DOMAIN\User) | User identity |
| MachineName | string | Yes | Computer where exported from | Hostname |
| Rules | List\<IndexRule\> | Yes | Collection of index rules | Can be empty list |
| ExtensionSettings | List\<FileExtensionSetting\> | Yes | Collection of extension configs | Can be empty list |
| Checksum | string | No | SHA256 hash for integrity | Optional validation |

#### JSON Schema

See `/contracts/configuration-schema.json` for full JSON schema definition.

#### Business Rules

- Version must match current application version for import
- Older versions may be upgradable via migration logic
- Export includes timestamp and user for audit trail
- Import validates all rules before applying any (atomic operation)
- Partial imports allowed with `--continue-on-error` flag

---

## Enumerations

### RuleType

```csharp
public enum RuleType
{
    Include = 0,  // Index this location
    Exclude = 1   // Don't index this location
}
```

### RuleSource

```csharp
public enum RuleSource
{
    System = 0,    // Windows default rule
    User = 1,      // User-created rule
    Imported = 2   // Imported from config file
}
```

### FilterType

```csharp
public enum FilterType
{
    Include = 0,  // Include matching items
    Exclude = 1   // Exclude matching items
}
```

### FilterTarget

```csharp
public enum FilterTarget
{
    FileExtension = 0,  // Pattern matches file extensions (*.txt)
    FileName = 1,       // Pattern matches file names (readme.*)
    Subfolder = 2       // Pattern matches subfolder names (node_modules)
}
```

### PathType

```csharp
public enum PathType
{
    Local = 0,      // C:\folder or D:\path
    UNC = 1,        // \\server\share\path
    Relative = 2    // .\subfolder or ..\parent
}
```

### IndexingDepth

```csharp
public enum IndexingDepth
{
    NotIndexed = 0,              // Extension not indexed at all
    PropertiesOnly = 1,          // Index metadata only
    PropertiesAndContents = 2    // Index metadata + full-text content
}
```

---

## Value Objects

### ValidationResult

Represents the outcome of a validation operation.

```csharp
public class ValidationResult
{
    public bool IsValid { get; init; }
    public string NormalizedValue { get; init; }
    public string ErrorMessage { get; init; }
    public ValidationSeverity Severity { get; init; }
    
    public static ValidationResult Success(string normalizedValue) => new()
    {
        IsValid = true,
        NormalizedValue = normalizedValue,
        Severity = ValidationSeverity.None
    };
    
    public static ValidationResult Failure(string error) => new()
    {
        IsValid = false,
        ErrorMessage = error,
        Severity = ValidationSeverity.Error
    };
    
    public static ValidationResult Warning(string warning, string normalizedValue) => new()
    {
        IsValid = true,
        NormalizedValue = normalizedValue,
        ErrorMessage = warning,
        Severity = ValidationSeverity.Warning
    };
}

public enum ValidationSeverity
{
    None = 0,
    Warning = 1,
    Error = 2
}
```

### OperationResult

Represents the outcome of an operation with detailed context.

```csharp
public class OperationResult
{
    public bool Success { get; init; }
    public string Message { get; init; }
    public string SuggestedAction { get; init; }
    public Exception Exception { get; init; }
    public object Data { get; init; }
    
    public static OperationResult Succeeded(string message, object data = null) => new()
    {
        Success = true,
        Message = message,
        Data = data
    };
    
    public static OperationResult Failed(string message, string suggestedAction, Exception ex = null) => new()
    {
        Success = false,
        Message = message,
        SuggestedAction = suggestedAction,
        Exception = ex
    };
}
```

---

## Domain Invariants

### Cross-Entity Rules

1. **Rule Uniqueness**: Only one IndexRule per unique path
   - Validation: Check existing rules before adding new rule
   - Enforcement: At service layer before persistence

2. **Privilege Requirements**:
   - Read operations: No admin required
   - Write operations: Admin required
   - Validation: At command handler level

3. **Path Constraints**:
   - All paths must resolve to absolute paths
   - Path length ≤ 260 characters
   - UNC paths must be well-formed
   - Validation: At PathValidator service

4. **Filter Consistency**:
   - Include filters only apply to Include rules
   - Exclude filters can apply to any rule
   - Subfolder exclusions only apply to Recursive rules
   - Validation: At IndexRule entity level

5. **Extension Settings**:
   - Extensions must start with dot (`.txt`)
   - Case-insensitive comparison
   - Cannot change default Windows settings (read-only)
   - Validation: At FileExtensionSetting entity level

---

## Entity Relationships Diagram

```
┌─────────────┐
│ IndexRule   │
├─────────────┤
│ + Id        │
│ + Path      │──────┐
│ + RuleType  │      │
│ + Recursive │      │ references
└─────────────┘      │
       │             │
       │ contains    ▼
       │       ┌──────────────┐
       │       │ IndexLocation│
       │       ├──────────────┤
       │       │ + FullPath   │
       │       │ + PathType   │
       │       │ + Exists     │
       │       └──────────────┘
       │
       │ 0..*
       ▼
┌──────────────────┐
│ FileTypeFilter   │
├──────────────────┤
│ + Pattern        │
│ + FilterType     │
│ + AppliesTo      │
└──────────────────┘


┌─────────────────────────┐
│ FileExtensionSetting    │
├─────────────────────────┤
│ + Extension             │
│ + IndexingDepth         │
│ + IsDefaultSetting      │
└─────────────────────────┘
      ▲
      │ many
      │
┌─────────────────┐
│ ConfigurationFile│
├─────────────────┤
│ + Version       │
│ + ExportDate    │
│ + Rules         │───┐
│ + Extensions    │   │
└─────────────────┘   │
                      │ contains many
                      ▼
              ┌─────────────┐
              │ IndexRule   │
              └─────────────┘
```

---

## Persistence Mapping

### Windows Search COM API Mapping

| Entity | COM Interface | Method | Notes |
|--------|---------------|--------|-------|
| IndexRule | ISearchCrawlScopeManager | AddUserScopeRule() | Creates new rule |
| IndexRule | ISearchCrawlScopeManager | RemoveScopeRule() | Deletes rule |
| IndexRule | ISearchCrawlScopeManager | EnumerateScopeRules() | Lists all rules |
| FileExtensionSetting | Registry | HKLM\...\FileTypes\{ext} | DWORD value (0/1/2) |

### JSON Serialization

All entities serialize to/from JSON using System.Text.Json with these conventions:
- camelCase property names
- ISO 8601 dates
- Enums as strings
- Collections as arrays

---

## Model Validation Summary

| Entity | Validation Points | Enforced By |
|--------|-------------------|-------------|
| IndexRule | Path format, length, uniqueness | PathValidator, IndexRuleValidator |
| FileTypeFilter | Pattern syntax, wildcard validity | FilterValidator |
| IndexLocation | Path existence, accessibility | PathValidator |
| FileExtensionSetting | Extension format, depth value | ExtensionValidator |
| ConfigurationFile | Schema version, rule validity | ConfigurationValidator |

All validators return `ValidationResult` value objects with detailed error messages and suggested actions.

---

## Next Steps

1. Generate API contracts in `/contracts/` directory
2. Create quickstart.md with usage examples
3. Update agent context with technology stack
4. Proceed to Phase 2: Tasks decomposition
