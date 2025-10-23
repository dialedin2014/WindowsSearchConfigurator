# Tasks: Windows Search Configurator

## Task Breakdown

**Input**: Design documents from `/specs/001-windows-search-configurator/`
**Prerequisites**: plan.md, spec.md, research.md, data-model.md, contracts/

**Tests**: Not explicitly requested in feature specification - focusing on implementation tasks

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Project initialization and basic structure

- [ ] T001 Create solution file WindowsSearchConfigurator.sln at repository root
- [ ] T002 Create src/WindowsSearchConfigurator/WindowsSearchConfigurator.csproj targeting .NET 8.0
- [ ] T003 [P] Create tests/WindowsSearchConfigurator.UnitTests/WindowsSearchConfigurator.UnitTests.csproj with NUnit, Moq, FluentAssertions references
- [ ] T004 [P] Create tests/WindowsSearchConfigurator.IntegrationTests/WindowsSearchConfigurator.IntegrationTests.csproj
- [ ] T005 [P] Create tests/WindowsSearchConfigurator.ContractTests/WindowsSearchConfigurator.ContractTests.csproj
- [ ] T006 Add package references to src/WindowsSearchConfigurator.csproj: System.Management, System.CommandLine, System.Text.Json
- [ ] T007 [P] Create src/WindowsSearchConfigurator/Core/Models/ directory structure
- [ ] T008 [P] Create src/WindowsSearchConfigurator/Core/Interfaces/ directory structure
- [ ] T009 [P] Create src/WindowsSearchConfigurator/Services/ directory structure
- [ ] T010 [P] Create src/WindowsSearchConfigurator/Infrastructure/ directory structure
- [ ] T011 [P] Create src/WindowsSearchConfigurator/Utilities/ directory structure
- [ ] T012 [P] Create src/WindowsSearchConfigurator/Commands/ directory structure
- [ ] T013 [P] Create .editorconfig at repository root with C# coding standards
- [ ] T014 [P] Create .gitignore with standard .NET exclusions (bin/, obj/, etc.)

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core infrastructure that MUST be complete before ANY user story can be implemented

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

- [ ] T015 [P] Create RuleType enum in src/WindowsSearchConfigurator/Core/Models/RuleType.cs (Include, Exclude)
- [ ] T016 [P] Create RuleSource enum in src/WindowsSearchConfigurator/Core/Models/RuleSource.cs (System, User, Imported)
- [ ] T017 [P] Create FilterType enum in src/WindowsSearchConfigurator/Core/Models/FilterType.cs (Include, Exclude)
- [ ] T018 [P] Create FilterTarget enum in src/WindowsSearchConfigurator/Core/Models/FilterTarget.cs (FileExtension, FileName, Subfolder)
- [ ] T019 [P] Create PathType enum in src/WindowsSearchConfigurator/Core/Models/PathType.cs (Local, UNC, Relative)
- [ ] T020 [P] Create IndexingDepth enum in src/WindowsSearchConfigurator/Core/Models/IndexingDepth.cs (NotIndexed, PropertiesOnly, PropertiesAndContents)
- [ ] T021 [P] Create ValidationResult value object in src/WindowsSearchConfigurator/Core/Models/ValidationResult.cs
- [ ] T022 [P] Create OperationResult value object in src/WindowsSearchConfigurator/Core/Models/OperationResult.cs
- [ ] T023 [P] Create FileTypeFilter model in src/WindowsSearchConfigurator/Core/Models/FileTypeFilter.cs
- [ ] T024 Create IndexLocation model in src/WindowsSearchConfigurator/Core/Models/IndexLocation.cs (depends on PathType)
- [ ] T025 Create IndexRule model in src/WindowsSearchConfigurator/Core/Models/IndexRule.cs (depends on RuleType, RuleSource, FileTypeFilter)
- [ ] T026 [P] Create FileExtensionSetting model in src/WindowsSearchConfigurator/Core/Models/FileExtensionSetting.cs (depends on IndexingDepth)
- [ ] T027 [P] Create ConfigurationFile model in src/WindowsSearchConfigurator/Core/Models/ConfigurationFile.cs (depends on IndexRule, FileExtensionSetting)
- [ ] T028 [P] Create ISearchIndexManager interface in src/WindowsSearchConfigurator/Core/Interfaces/ISearchIndexManager.cs
- [ ] T029 [P] Create IConfigurationStore interface in src/WindowsSearchConfigurator/Core/Interfaces/IConfigurationStore.cs
- [ ] T030 [P] Create IAuditLogger interface in src/WindowsSearchConfigurator/Core/Interfaces/IAuditLogger.cs
- [ ] T031 [P] Create IPrivilegeChecker interface in src/WindowsSearchConfigurator/Core/Interfaces/IPrivilegeChecker.cs
- [ ] T032 Implement PrivilegeChecker service in src/WindowsSearchConfigurator/Services/PrivilegeChecker.cs using WindowsPrincipal.IsInRole
- [ ] T033 Implement AuditLogger service in src/WindowsSearchConfigurator/Services/AuditLogger.cs with file-based logging
- [ ] T034 Implement PathValidator service in src/WindowsSearchConfigurator/Services/PathValidator.cs with UNC/local/relative path support and MAX_PATH validation
- [ ] T035 [P] Create WindowsSearchInterop wrapper in src/WindowsSearchConfigurator/Infrastructure/WindowsSearchInterop.cs for COM API access
- [ ] T036 [P] Create RegistryAccessor wrapper in src/WindowsSearchConfigurator/Infrastructure/RegistryAccessor.cs for registry operations
- [ ] T037 [P] Create ServiceStatusChecker in src/WindowsSearchConfigurator/Infrastructure/ServiceStatusChecker.cs to verify Windows Search service status
- [ ] T038 [P] Create WildcardMatcher utility in src/WindowsSearchConfigurator/Utilities/WildcardMatcher.cs for pattern matching
- [ ] T039 [P] Create PathNormalizer utility in src/WindowsSearchConfigurator/Utilities/PathNormalizer.cs for path handling
- [ ] T040 [P] Create ConsoleFormatter utility in src/WindowsSearchConfigurator/Utilities/ConsoleFormatter.cs for table/JSON/CSV output
- [ ] T041 Create Program.cs entry point in src/WindowsSearchConfigurator/Program.cs with DI container setup and System.CommandLine root command

**Checkpoint**: Foundation ready - user story implementation can now begin in parallel

---

## Phase 3: User Story 1 - View Current Index Rules (Priority: P1) 🎯 MVP

**Goal**: Enable users to view all Windows Search index rules including paths, types, filters, and extension settings

**Independent Test**: Run the application with list command and verify current Windows Search index rules are displayed correctly. Test with different output formats (table, JSON, CSV). Verify works without admin privileges.

### Implementation for User Story 1

- [ ] T042 [US1] Implement SearchIndexManager.GetAllRules() in src/WindowsSearchConfigurator/Services/SearchIndexManager.cs using COM API ISearchCrawlScopeManager.EnumerateScopeRules()
- [ ] T043 [US1] Implement SearchIndexManager.GetExtensionSettings() in src/WindowsSearchConfigurator/Services/SearchIndexManager.cs reading from Registry HKLM\SOFTWARE\Microsoft\Windows Search\Preferences\FileTypes
- [ ] T044 [US1] Create ListCommand handler in src/WindowsSearchConfigurator/Commands/ListCommand.cs with options for format (table/json/csv), show-defaults, and filter
- [ ] T045 [US1] Add ListCommand to root command configuration in src/WindowsSearchConfigurator/Program.cs
- [ ] T046 [US1] Implement table formatting for list output in ConsoleFormatter with column alignment and Unicode box-drawing
- [ ] T047 [US1] Implement JSON formatting for list output in ConsoleFormatter using System.Text.Json
- [ ] T048 [US1] Implement CSV formatting for list output in ConsoleFormatter following RFC 4180

**Checkpoint**: At this point, User Story 1 should be fully functional - users can view all index rules in multiple formats without admin privileges

---

## Phase 4: User Story 2 - Add New Index Rules (Priority: P2)

**Goal**: Enable administrators to add folders to Windows Search index with file type filters, exclusion patterns, and recursive/non-recursive options

**Independent Test**: Run add command as administrator with a test folder path. Verify the folder is added to Windows Search configuration. Test with file type filters, exclusion patterns, and non-recursive flag. Confirm Windows Search begins indexing the location.

### Implementation for User Story 2

- [ ] T049 [P] [US2] Create AddCommand handler in src/WindowsSearchConfigurator/Commands/AddCommand.cs with path argument and options (non-recursive, include, exclude-files, exclude-folders, type)
- [ ] T050 [US2] Implement SearchIndexManager.AddIndexRule() in src/WindowsSearchConfigurator/Services/SearchIndexManager.cs using COM API ISearchCrawlScopeManager.AddUserScopeRule()
- [ ] T051 [US2] Add privilege checking in AddCommand handler using IPrivilegeChecker before attempting modification
- [ ] T052 [US2] Add path validation in AddCommand handler using PathValidator before adding rule
- [ ] T053 [US2] Implement duplicate rule detection in SearchIndexManager.AddIndexRule() with user confirmation prompt
- [ ] T054 [US2] Add audit logging in AddCommand handler using IAuditLogger after successful rule addition
- [ ] T055 [US2] Add AddCommand to root command configuration in src/WindowsSearchConfigurator/Program.cs
- [ ] T056 [US2] Implement error handling in AddCommand for COM exceptions (0x80040D03 for duplicate, 0x80070005 for access denied)

**Checkpoint**: At this point, User Stories 1 AND 2 should both work independently - users can view rules and administrators can add new rules

---

## Phase 5: User Story 3 - Remove Index Rules (Priority: P3)

**Goal**: Enable administrators to remove folders from Windows Search index with confirmation prompts for safety

**Independent Test**: Run remove command as administrator with an indexed folder path. Confirm the confirmation prompt appears. Accept the prompt and verify the folder is removed from Windows Search configuration.

### Implementation for User Story 3

- [ ] T057 [P] [US3] Create RemoveCommand handler in src/WindowsSearchConfigurator/Commands/RemoveCommand.cs with path argument and force option
- [ ] T058 [US3] Implement SearchIndexManager.RemoveIndexRule() in src/WindowsSearchConfigurator/Services/SearchIndexManager.cs using COM API ISearchCrawlScopeManager.RemoveScopeRule()
- [ ] T059 [US3] Add privilege checking in RemoveCommand handler using IPrivilegeChecker
- [ ] T060 [US3] Implement confirmation prompt in RemoveCommand handler (skippable with --force or --no-confirm flags)
- [ ] T061 [US3] Add error handling for non-existent rule (COM exception 0x80040D04) with clear message
- [ ] T062 [US3] Add audit logging in RemoveCommand handler using IAuditLogger after successful rule removal
- [ ] T063 [US3] Add RemoveCommand to root command configuration in src/WindowsSearchConfigurator/Program.cs

**Checkpoint**: All basic CRUD operations now functional - users can view, add, and remove index rules

---

## Phase 6: User Story 4 - Modify Existing Index Rules (Priority: P4)

**Goal**: Enable administrators to update existing index rule properties without removing and re-adding rules

**Independent Test**: Run modify command as administrator on an existing rule. Change properties like recursive flag, file type filters, or rule type. Verify changes are reflected in Windows Search configuration using the list command.

### Implementation for User Story 4

- [ ] T064 [P] [US4] Create ModifyCommand handler in src/WindowsSearchConfigurator/Commands/ModifyCommand.cs with path argument and options (recursive, include, exclude-files, exclude-folders, type)
- [ ] T065 [US4] Implement SearchIndexManager.ModifyIndexRule() in src/WindowsSearchConfigurator/Services/SearchIndexManager.cs (remove old rule, add updated rule atomically)
- [ ] T066 [US4] Add privilege checking in ModifyCommand handler using IPrivilegeChecker
- [ ] T067 [US4] Implement rule existence check in ModifyCommand with helpful error message if rule not found
- [ ] T068 [US4] Add confirmation prompt in ModifyCommand handler for destructive changes
- [ ] T069 [US4] Add audit logging in ModifyCommand handler using IAuditLogger after successful modification
- [ ] T070 [US4] Add ModifyCommand to root command configuration in src/WindowsSearchConfigurator/Program.cs

**Checkpoint**: Full CRUD operations complete - users can view, add, remove, and modify index rules

---

## Phase 7: User Story 5 - Configure File Extension Indexing Depth (Priority: P5)

**Goal**: Enable administrators to search for file extensions with wildcards and configure indexing depth (Properties only vs Properties and Contents)

**Independent Test**: Run search-extensions command with wildcard patterns and verify matching extensions are displayed with their indexing depth. Run configure-depth command to change an extension's setting and verify the change with search-extensions.

### Implementation for User Story 5

- [ ] T071 [P] [US5] Create SearchExtensionsCommand handler in src/WindowsSearchConfigurator/Commands/SearchExtensionsCommand.cs with pattern argument and format/depth filter options
- [ ] T072 [P] [US5] Create ConfigureDepthCommand handler in src/WindowsSearchConfigurator/Commands/ConfigureDepthCommand.cs with extension and depth arguments
- [ ] T073 [US5] Implement SearchIndexManager.SearchExtensions() in src/WindowsSearchConfigurator/Services/SearchIndexManager.cs enumerating Registry FileTypes key with wildcard matching
- [ ] T074 [US5] Implement SearchIndexManager.SetExtensionDepth() in src/WindowsSearchConfigurator/Services/SearchIndexManager.cs writing DWORD to Registry (0=NotIndexed, 1=PropertiesOnly, 2=PropertiesAndContents)
- [ ] T075 [US5] Add privilege checking in ConfigureDepthCommand handler using IPrivilegeChecker
- [ ] T076 [US5] Implement wildcard pattern conversion in WildcardMatcher utility (*.log, *.*x patterns to regex)
- [ ] T077 [US5] Add audit logging in ConfigureDepthCommand handler using IAuditLogger after successful depth change
- [ ] T078 [US5] Add SearchExtensionsCommand to root command configuration in src/WindowsSearchConfigurator/Program.cs
- [ ] T079 [US5] Add ConfigureDepthCommand to root command configuration in src/WindowsSearchConfigurator/Program.cs

**Checkpoint**: Extension management complete - users can search and configure file extension indexing behavior

---

## Phase 8: User Story 6 - Batch Rule Operations (Priority: P6)

**Goal**: Enable users to export current rules to JSON and administrators to import rules from JSON for bulk operations and cross-system configuration

**Independent Test**: Run export command to create a JSON file with current configuration. Verify JSON matches schema. Run import command on another machine or after clearing rules and verify all rules are restored correctly. Test merge mode and continue-on-error options.

### Implementation for User Story 6

- [ ] T080 [P] [US6] Create ExportCommand handler in src/WindowsSearchConfigurator/Commands/ExportCommand.cs with file argument and options (include-defaults, include-extensions, overwrite)
- [ ] T081 [P] [US6] Create ImportCommand handler in src/WindowsSearchConfigurator/Commands/ImportCommand.cs with file argument and options (merge, continue-on-error, dry-run)
- [ ] T082 [US6] Implement ConfigurationStore.Export() in src/WindowsSearchConfigurator/Services/ConfigurationStore.cs serializing rules and extensions to JSON with System.Text.Json
- [ ] T083 [US6] Implement ConfigurationStore.Import() in src/WindowsSearchConfigurator/Services/ConfigurationStore.cs deserializing JSON with schema validation
- [ ] T084 [US6] Add JSON schema validation in ConfigurationStore.Import() checking version compatibility and required fields
- [ ] T085 [US6] Implement batch rule application in ImportCommand with error handling and reporting (success/failure per rule)
- [ ] T086 [US6] Add privilege checking in ImportCommand handler using IPrivilegeChecker (read-only export, admin for import)
- [ ] T087 [US6] Implement merge logic in ImportCommand to combine with existing rules instead of replacing
- [ ] T088 [US6] Implement dry-run mode in ImportCommand to validate without applying changes
- [ ] T089 [US6] Add audit logging in ExportCommand and ImportCommand handlers using IAuditLogger
- [ ] T090 [US6] Add ExportCommand to root command configuration in src/WindowsSearchConfigurator/Program.cs
- [ ] T091 [US6] Add ImportCommand to root command configuration in src/WindowsSearchConfigurator/Program.cs

**Checkpoint**: All user stories complete - full feature functionality delivered

---

## Phase 9: Polish & Cross-Cutting Concerns

**Purpose**: Improvements that affect multiple user stories and final documentation

- [ ] T092 [P] Add XML documentation comments to all public interfaces in src/WindowsSearchConfigurator/Core/Interfaces/
- [ ] T093 [P] Add XML documentation comments to all public models in src/WindowsSearchConfigurator/Core/Models/
- [ ] T094 [P] Implement --verbose flag support in Program.cs for detailed diagnostic output
- [ ] T095 [P] Implement --version flag handler in Program.cs displaying version, copyright, runtime info
- [ ] T096 [P] Add comprehensive error message templates with suggested actions in all command handlers
- [ ] T097 [P] Create README.md at repository root with project overview and quick start
- [ ] T098 [P] Verify all exit codes (0-5) are properly returned from command handlers per CLI contract
- [ ] T099 [P] Add progress indicators for long-running operations (batch import) in ImportCommand
- [ ] T100 [P] Validate all confirmation prompts follow consistent format with clear warnings
### T10: User Acceptance Testing (UAT)

- [ ] T101 Run through all scenarios in specs/001-windows-search-configurator/quickstart.md and verify functionality
- [ ] T102 Final code review: ensure all paths ≤260 chars, all COM calls have error handling, all write operations check privileges

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies - can start immediately
- **Foundational (Phase 2)**: Depends on Setup completion (T001-T014) - BLOCKS all user stories
- **User Stories (Phase 3-8)**: All depend on Foundational phase completion (T015-T041)
  - User stories can then proceed in parallel (if staffed)
  - Or sequentially in priority order (P1 → P2 → P3 → P4 → P5 → P6)
- **Polish (Phase 9)**: Depends on all user stories being complete (T042-T091)

### User Story Dependencies

- **User Story 1 (P1)**: Can start after Foundational (Phase 2) - No dependencies on other stories
- **User Story 2 (P2)**: Can start after Foundational (Phase 2) - Independent but builds on viewing capability
- **User Story 3 (P3)**: Can start after Foundational (Phase 2) - Independent but complements add functionality  
- **User Story 4 (P4)**: Can start after Foundational (Phase 2) - Independent modification capability
- **User Story 5 (P5)**: Can start after Foundational (Phase 2) - Independent extension management
- **User Story 6 (P6)**: Can start after Foundational (Phase 2) - Independent batch operations capability

### Within Each User Story

- US1: Tasks T042-T043 can run in parallel, T044 depends on them, T045-T048 are sequential refinements
- US2: Tasks T049-T050 foundation, T051-T056 build on them sequentially
- US3: Tasks T057-T058 foundation, T059-T063 build on them sequentially
- US4: Tasks T064-T065 foundation, T066-T070 build on them sequentially
- US5: Tasks T071-T072 can run in parallel, T073-T074 foundation, T075-T079 build sequentially
- US6: Tasks T080-T081 can run in parallel, T082-T083 foundation, T084-T091 build sequentially

### Parallel Opportunities

- **Setup Phase**: Tasks T003, T004, T005, T007-T012, T013, T014 can all run in parallel after T001-T002
- **Foundational Phase**: All enum/model creation tasks T015-T027 can run in parallel, interfaces T028-T031 can run in parallel, services/infrastructure T032-T040 can run in parallel after models/interfaces complete
- **User Stories**: Once Foundational complete, all 6 user stories can be developed in parallel by different team members
- **Within US1**: T046, T047, T048 (formatting) can run in parallel after T044
- **Within US5**: T071, T072 (commands) can run in parallel
- **Within US6**: T080, T081 (commands) can run in parallel
- **Polish Phase**: Most tasks T092-T100 can run in parallel

---

## Parallel Example: Foundational Phase

```bash
# Launch all enum definitions together:
Task: "Create RuleType enum in src/WindowsSearchConfigurator/Core/Models/RuleType.cs"
Task: "Create RuleSource enum in src/WindowsSearchConfigurator/Core/Models/RuleSource.cs"
Task: "Create FilterType enum in src/WindowsSearchConfigurator/Core/Models/FilterType.cs"
Task: "Create FilterTarget enum in src/WindowsSearchConfigurator/Core/Models/FilterTarget.cs"
Task: "Create PathType enum in src/WindowsSearchConfigurator/Core/Models/PathType.cs"
Task: "Create IndexingDepth enum in src/WindowsSearchConfigurator/Core/Models/IndexingDepth.cs"

# Launch all interface definitions together:
Task: "Create ISearchIndexManager interface in src/WindowsSearchConfigurator/Core/Interfaces/ISearchIndexManager.cs"
Task: "Create IConfigurationStore interface in src/WindowsSearchConfigurator/Core/Interfaces/IConfigurationStore.cs"
Task: "Create IAuditLogger interface in src/WindowsSearchConfigurator/Core/Interfaces/IAuditLogger.cs"
Task: "Create IPrivilegeChecker interface in src/WindowsSearchConfigurator/Core/Interfaces/IPrivilegeChecker.cs"
```

---

## Parallel Example: Multiple User Stories

```bash
# Once Foundational phase completes, launch all user stories:
Developer A: User Story 1 (View) - Tasks T042-T048
Developer B: User Story 2 (Add) - Tasks T049-T056
Developer C: User Story 3 (Remove) - Tasks T057-T063
Developer D: User Story 4 (Modify) - Tasks T064-T070
Developer E: User Story 5 (Extensions) - Tasks T071-T079
Developer F: User Story 6 (Batch) - Tasks T080-T091
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup (T001-T014) - **~14 tasks**
2. Complete Phase 2: Foundational (T015-T041) - **~27 tasks, CRITICAL**
3. Complete Phase 3: User Story 1 (T042-T048) - **~7 tasks**
4. **STOP and VALIDATE**: Test viewing rules independently
5. Deploy/demo read-only audit tool (provides immediate value)

**MVP Scope**: 48 tasks total for a working view-only tool

### Incremental Delivery

1. Complete Setup + Foundational → Foundation ready (41 tasks)
2. Add User Story 1 → Test independently → Deploy/Demo (7 tasks - MVP! 🎯)
3. Add User Story 2 → Test independently → Deploy/Demo (8 tasks - can add rules)
4. Add User Story 3 → Test independently → Deploy/Demo (7 tasks - can remove rules)
5. Add User Story 4 → Test independently → Deploy/Demo (7 tasks - can modify rules)
6. Add User Story 5 → Test independently → Deploy/Demo (9 tasks - extension control)
7. Add User Story 6 → Test independently → Deploy/Demo (12 tasks - batch operations)
8. Polish phase → Final refinements (11 tasks)

**Total**: 102 tasks for complete feature

### Parallel Team Strategy

With 6 developers after Foundational phase completes:

1. Team completes Setup + Foundational together (41 tasks)
2. Once Foundational is done (CHECKPOINT):
   - Developer A: User Story 1 - View (7 tasks)
   - Developer B: User Story 2 - Add (8 tasks)
   - Developer C: User Story 3 - Remove (7 tasks)
   - Developer D: User Story 4 - Modify (7 tasks)
   - Developer E: User Story 5 - Extensions (9 tasks)
   - Developer F: User Story 6 - Batch (12 tasks)
3. Stories complete and integrate independently
4. Polish phase together (11 tasks)

---

## Task Count Summary

- **Phase 1 - Setup**: 14 tasks
- **Phase 2 - Foundational**: 27 tasks (BLOCKS all user stories)
- **Phase 3 - User Story 1 (P1)**: 7 tasks 🎯 MVP
- **Phase 4 - User Story 2 (P2)**: 8 tasks
- **Phase 5 - User Story 3 (P3)**: 7 tasks
- **Phase 6 - User Story 4 (P4)**: 7 tasks
- **Phase 7 - User Story 5 (P5)**: 9 tasks
- **Phase 8 - User Story 6 (P6)**: 12 tasks
- **Phase 9 - Polish**: 11 tasks

**Total Tasks**: 102

**Parallel Opportunities**: 35+ tasks can run in parallel (marked with [P])

**Independent Test Criteria**: Each user story phase includes specific test scenarios from spec.md

**Suggested MVP Scope**: Phases 1-3 (Setup + Foundational + User Story 1) = 48 tasks for view-only audit tool

---

## Notes

- All tasks follow strict checkbox format: `- [ ] [ID] [P?] [Story?] Description with file path`
- Tasks organized by user story to enable independent implementation and testing
- Each user story can be validated independently before moving to next priority
- Foundational phase MUST complete before any user story work begins
- No test tasks included as tests were not explicitly requested in feature specification
- Commit after each task or logical group
- Constitution compliance: All principles followed (automated testing via contract, error handling, user control, clear interfaces, documentation, incremental implementation)
