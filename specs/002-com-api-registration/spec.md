# Feature Specification: COM API Registration Support

**Feature Branch**: `002-com-api-registration`  
**Created**: October 22, 2025  
**Status**: Draft  
**Input**: User description: "If the Microsoft.Search.Interop.CSearchManager COM API is not registered on the system, offer to register it."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Detection and Notification (Priority: P1)

When a system administrator runs WindowsSearchConfigurator on a machine where the Microsoft.Search.Interop.CSearchManager COM API is not registered, they receive a clear notification explaining that the COM API is missing and that the tool cannot function without it.

**Why this priority**: This is the foundation of the feature - users must be informed of the problem before any action can be taken. Without detection, the tool would simply fail with cryptic errors.

**Independent Test**: Can be fully tested by running the tool on a system without the COM API registered and verifying that a clear, informative message is displayed (no cryptic error codes or stack traces).

**Acceptance Scenarios**:

1. **Given** the COM API is not registered on the system, **When** the user runs any WindowsSearchConfigurator command, **Then** the system detects the missing registration and displays a clear error message explaining the issue
2. **Given** the COM API is properly registered, **When** the user runs WindowsSearchConfigurator commands, **Then** the system proceeds normally without any registration-related messages

---

### User Story 2 - Interactive Registration Offer (Priority: P2)

When the COM API is detected as unregistered, the administrator is offered an option to attempt automatic registration with clear guidance on what will happen.

**Why this priority**: This provides the core value-add of the feature - automated remediation. Without this, users would need to manually figure out how to register the COM API.

**Independent Test**: Can be tested by simulating an unregistered state, running the tool, and verifying that the user is prompted with a clear yes/no option to register the API.

**Acceptance Scenarios**:

1. **Given** the COM API is not registered, **When** the detection notification is shown, **Then** the user is presented with an option to attempt automatic registration
2. **Given** the user is offered registration, **When** they decline the offer, **Then** the tool exits gracefully with guidance on manual registration steps
3. **Given** the user is offered registration, **When** they accept the offer, **Then** the tool proceeds to attempt registration with appropriate privilege checking

---

### User Story 3 - Privilege-Aware Registration (Priority: P2)

When the user accepts the registration offer, the system checks for administrative privileges and either performs the registration or provides clear guidance on how to elevate privileges.

**Why this priority**: COM registration requires administrative privileges. This ensures the tool handles privilege requirements gracefully rather than failing with access denied errors.

**Independent Test**: Can be tested by running the tool without admin privileges and verifying that it detects the lack of privileges and provides appropriate guidance (rather than attempting and failing).

**Acceptance Scenarios**:

1. **Given** the user accepts the registration offer and has administrative privileges, **When** the registration is attempted, **Then** the system registers the COM API successfully
2. **Given** the user accepts the registration offer but lacks administrative privileges, **When** the privilege check is performed, **Then** the system displays instructions on how to run the tool as administrator
3. **Given** the registration attempt fails due to file not found or other errors, **When** the error occurs, **Then** the system displays a meaningful error message with troubleshooting steps

---

### User Story 4 - Non-Interactive Mode Support (Priority: P3)

When the tool is run in automated/scripted scenarios (CI/CD pipelines, deployment scripts), it can be configured to handle COM API registration without user interaction through command-line flags.

**Why this priority**: While important for automation scenarios, this is not critical for the primary use case of interactive system administration. Most users will run the tool interactively.

**Independent Test**: Can be tested by running the tool with a `--auto-register-com` flag (or similar) and verifying that it attempts registration without prompting for confirmation.

**Acceptance Scenarios**:

1. **Given** the tool is run with an auto-registration flag and the COM API is not registered, **When** the detection occurs, **Then** the system automatically attempts registration without prompting
2. **Given** the tool is run with a no-registration flag and the COM API is not registered, **When** the detection occurs, **Then** the system exits with an error code without attempting registration

---

### Edge Cases

- What happens when the COM API DLL exists but is not properly registered in the registry?
- How does the system handle partial registration (some registry keys present, others missing)?
- What happens if the registration attempt is interrupted (power loss, user cancellation)?
- How does the system behave if the COM API DLL version is incompatible with the current Windows version?
- What happens when multiple instances of the tool attempt registration simultaneously?
- How does the system handle registration on Windows Server Core (no GUI) versus desktop Windows?

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST detect whether the Microsoft.Search.Interop.CSearchManager COM API is registered before attempting to use it
- **FR-002**: System MUST display a clear, non-technical error message when the COM API is not registered, explaining what the issue is and what impact it has
- **FR-003**: System MUST offer the user an option to attempt automatic COM API registration when the API is detected as unregistered
- **FR-004**: System MUST check for administrative privileges before attempting COM API registration
- **FR-005**: System MUST provide clear instructions on how to elevate privileges if registration is attempted without administrative rights
- **FR-006**: System MUST attempt to register the COM API using the appropriate Windows registration mechanism (regsvr32 or equivalent programmatic approach) when the user accepts the offer
- **FR-007**: System MUST report the outcome of the registration attempt (success, failure, reason for failure) to the user in clear terms
- **FR-008**: System MUST provide fallback guidance for manual COM API registration if automatic registration fails
- **FR-009**: System MUST support a command-line option for non-interactive automatic registration (for scripted/automated scenarios)
- **FR-010**: System MUST support a command-line option to skip registration attempts and exit with an error when COM API is not registered
- **FR-011**: System MUST validate that the COM API is functional after registration before proceeding with normal operations
- **FR-012**: System MUST log all COM API registration attempts, including success/failure status and any error messages, for troubleshooting purposes

### Key Entities

- **COM API Registration Status**: Represents the current state of the Microsoft.Search.Interop.CSearchManager COM API registration, including whether it's registered, whether the DLL is present, and whether it's functional
- **Registration Attempt**: Represents a single attempt to register the COM API, including timestamp, outcome, error details, user context (interactive vs automated), and privilege level

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: When the COM API is not registered, 100% of users receive a clear notification explaining the issue in non-technical terms
- **SC-002**: Users can complete the entire registration process (from detection to successful registration) in under 1 minute when running with administrative privileges
- **SC-003**: At least 90% of automatic registration attempts succeed on supported Windows versions when proper privileges are available
- **SC-004**: Zero instances of cryptic error messages (COM error codes, CLSIDs, stack traces) shown to users during the registration flow
- **SC-005**: System administrators can deploy the tool in automated environments with zero manual intervention required for COM API registration
- **SC-006**: 100% of registration attempts are logged with sufficient detail to troubleshoot failures

## Assumptions

- The Microsoft.Search.Interop.CSearchManager COM API DLL is present on the system (installed as part of Windows Search service) but may not be registered
- Users running WindowsSearchConfigurator have the ability to run the tool as administrator when needed
- The COM API registration process follows standard Windows COM registration patterns (registry entries, DLL registration)
- The target Windows versions (Windows 10/11, Windows Server 2016+) all support the same COM API registration mechanism
- Registration failures are typically due to permissions issues or missing/corrupted files, not architectural incompatibilities

## Dependencies

- Windows Search service must be installed on the target system (though it doesn't need to be running for registration)
- Administrative privileges are required for COM registration
- The searchapi.dll (or equivalent) must be present in the Windows system directory

## Out of Scope

- Installing the Windows Search service if it's not present on the system
- Repairing corrupted Windows Search installations
- Registering other COM APIs beyond Microsoft.Search.Interop.CSearchManager
- Providing a GUI wizard for COM registration (command-line interface only)
- Automatically elevating privileges through UAC prompts (users must manually run as administrator)
- Backward compatibility with Windows versions older than Windows 10/Server 2016
