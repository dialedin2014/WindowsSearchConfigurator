<!--
Sync Impact Report:
- Version change: 1.3.0 → 1.3.1
- Modified principles: VII. Source Control Discipline - Added GitHub CLI requirement
- Added sections: 
  - Technical Standards > Tooling (GitHub CLI)
  - Project Naming Standard
- Removed sections: NONE
- Templates requiring updates:
  ✅ plan-template.md - Constitution Check section aligns with principles
  ✅ spec-template.md - User scenarios and requirements sections support testing
  ✅ tasks-template.md - Task organization supports test-first and incremental approach
- Specification files: No updates required (tooling is process-level)
- Follow-up TODOs: NONE
-->

# Windows Search Configurator Constitution

## Core Principles

### I. Automated Testing (NON-NEGOTIABLE)

All production code MUST have automated tests. Test categories required:

- **Unit tests**: Test individual components and functions in isolation
- **Integration tests**: Test component interactions and Windows Search API integration
- **Contract tests**: Validate public interfaces and API contracts
- **Test-first approach preferred**: Write tests before implementation when feasible

**Rationale**: Windows Search indexing is critical system functionality. Automated tests ensure reliability, prevent regressions, and enable confident refactoring. Testing Windows API integration is essential to verify correct behavior across Windows versions.

### II. Windows API Safety

All Windows Search API calls MUST include proper error handling and validation:

- Check API return codes and handle failures gracefully
- Validate input parameters before API calls
- Include appropriate error messages for troubleshooting
- Document Windows version compatibility requirements

**Rationale**: Windows Search APIs can fail or behave differently across Windows versions. Proper error handling prevents system instability and provides clear diagnostics.

### III. User Configuration Control

Users MUST maintain full control over search indexing configuration:

- All changes require explicit user approval
- Provide clear preview of changes before applying
- Support undo/rollback of configuration changes
- Never modify settings silently or automatically

**Rationale**: Search indexing directly impacts system performance and privacy. Users must understand and explicitly approve all configuration changes.

### IV. Clear Interface Design

All user interfaces (CLI, GUI, or API) MUST be clear and discoverable:

- Use self-documenting names and clear terminology
- Provide helpful error messages with actionable guidance
- Include inline help and documentation
- Follow Windows UI conventions where applicable

**Rationale**: Windows Search configuration is complex. Clear interfaces reduce user errors and support both novice and advanced users.

### V. Documentation and Maintainability

Code MUST be documented and maintainable:

- Public APIs have complete documentation
- Complex logic includes explanatory comments
- README explains setup, usage, and troubleshooting
- Architecture decisions are documented with rationale

**Rationale**: Windows Search APIs are complex and change across Windows versions. Clear documentation enables long-term maintenance and community contributions.

### VI. Incremental Implementation (NON-NEGOTIABLE)

Implementation tasks MUST be sized to prevent AI model context window overflow:

- Each implementation batch limited to **4-6 files maximum**
- Break large features into multiple incremental batches
- Each batch MUST be independently testable and reviewable
- Prefer smaller, focused changes over large monolithic updates
- Task lists organize work into digestible implementation phases

**Rationale**: AI-assisted development requires careful context management. Large file sets overwhelm model context windows, leading to incomplete implementations, lost context, and errors. Limiting batch size ensures high-quality code generation, better review processes, and maintains code coherence throughout implementation.

### VII. Source Control Discipline (NON-NEGOTIABLE)

All development MUST follow disciplined source control practices:

- **Use GitHub** for all source control operations
- **Use GitHub CLI** (`gh`) for repository interactions (branches, PRs, issues)
- **Commit frequently** with clear, descriptive commit messages
- **Interactive branch naming**: AI agent MUST prompt user for branch names before creating branches (never auto-generate)
- Use conventional commit format: `type(scope): description` (e.g., `feat(cli): add list command`, `fix(search): handle null paths`)
- Each commit should represent a logical, atomic unit of work
- Never commit broken code or failing tests to feature branches

**Rationale**: Frequent commits with clear messages create detailed project history, enable easy rollback, and facilitate code review. User-provided branch names ensure meaningful organization and avoid generic names like "feature-1" or "implementation-branch". GitHub CLI provides efficient command-line workflows for repository management. GitHub provides industry-standard collaboration tools, issue tracking, and automation capabilities.

## Technical Standards

### Project Naming Standard

**Official Name**: Windows Search Configurator

- Use "Windows Search Configurator" in all user-facing documentation
- Use "WindowsSearchConfigurator" (no spaces) for:
  - Repository names
  - Folder names
  - Assembly names (.csproj, .dll)
  - Namespace roots
  - Executable names (WindowsSearchConfigurator.exe)
- Use kebab-case "windows-search-configurator" for:
  - Branch names
  - Package identifiers
  - Docker image names (if applicable)
- Never use alternative names like "Windows Search Manager" or "SearchIndexer"

**Rationale**: Consistent naming across all artifacts ensures clarity, prevents confusion, and maintains professional standards. The official name "Windows Search Configurator" accurately describes the tool's purpose.

### Technology Stack

- **Platform**: Windows 10/11 (primary target)
- **Language**: TBD based on feature specification (C#, PowerShell, Python, or other)
- **Windows APIs**: Windows Search API, Registry access as needed
- **Testing Framework**: NUnit (standardized across all C# projects)

### Tooling

- **GitHub CLI**: `gh` (required for repository operations)
  - Install: `winget install GitHub.cli` or `choco install gh`
  - Authenticate: `gh auth login`
  - Common operations: `gh pr create`, `gh issue list`, `gh repo view`

### Source Control

- **Platform**: GitHub
- **CLI Tool**: GitHub CLI (`gh`) for all repository interactions
- **Branch Strategy**: Feature branches from default branch (master/main)
- **Branch Naming**: User-provided names (AI agent MUST prompt, never auto-generate)
- **Commit Convention**: Conventional Commits format (type(scope): description)
- **Commit Types**: feat, fix, docs, test, refactor, chore, ci, perf
- **Commit Frequency**: After each logical unit of work (multiple commits per session)

### CI/CD Pipeline

- **Platform**: GitHub Actions
- **Triggers**: Push to feature branches, pull requests to default branch
- **Required Checks**:
  - Build verification (compile/syntax check)
  - All automated tests (unit, integration, contract)
  - Code quality analysis (linting, static analysis)
- **Deployment**: Manual approval required for production releases
- **Test Environment**: Windows-based runners for integration tests

### Code Quality

- Follow language-specific style guides and conventions
- Use static analysis tools appropriate for chosen language
- Maintain consistent code formatting
- Keep functions focused and testable

### Security

- Request minimum necessary permissions
- Validate all user input
- Follow Windows security best practices
- Document security considerations for privileged operations

## Quality Gates

Before merging any feature or fix:

1. **All automated tests MUST pass** (blocking requirement)
2. **GitHub Actions CI pipeline MUST pass** (blocking requirement)
3. Code review completed with approval
4. Documentation updated to reflect changes
5. No regressions in existing functionality
6. Security review for privileged operations
7. Commit history is clean with conventional commit messages

## Governance

This constitution supersedes all other development practices. All features, changes, and pull requests MUST comply with these principles.

### Amendment Process

1. Propose amendment with clear rationale
2. Document impact on existing code and practices
3. Update affected templates and documentation
4. Version bump according to semantic versioning:
   - **MAJOR**: Backward incompatible principle changes
   - **MINOR**: New principles or material expansions
   - **PATCH**: Clarifications and refinements

### Compliance

- All specifications created with `/speckit.specify` MUST include testable acceptance criteria
- All implementation plans from `/speckit.plan` MUST include testing approach
- All task lists from `/speckit.tasks` MUST include test tasks organized in batches of 4-6 files
- Implementation via `/speckit.implement` MUST verify tests pass and respect file batch limits
- AI agents MUST prompt user for branch names before creating branches (never auto-generate)
- AI agents MUST create frequent commits with conventional commit messages
- All code changes MUST go through GitHub pull request workflow with CI checks

### Complexity Justification

Any deviation from these principles requires explicit justification documented in the implementation plan's "Complexity Tracking" section.

**Version**: 1.3.1 | **Ratified**: 2025-10-22 | **Last Amended**: 2025-10-22
