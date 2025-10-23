# Quickstart: COM API Registration Support

**Feature**: COM API Registration Support  
**Branch**: `002-com-api-registration`  
**Date**: 2025-10-22  
**Status**: Complete

## Overview

This quickstart guide helps developers implement and test the COM API registration support feature in WindowsSearchConfigurator. It covers setup, development workflow, testing strategies, and troubleshooting.

## Prerequisites

Before implementing this feature, ensure you have:

- **Development Environment**:
  - Visual Studio 2022 or later (with .NET 8.0 SDK)
  - Windows 10 (1809+), Windows 11, or Windows Server 2016+
  - Administrative privileges for integration testing
  - Git and GitHub CLI (`gh`) configured

- **Existing Project Knowledge**:
  - Familiarity with WindowsSearchConfigurator architecture
  - Understanding of existing services (AuditLogger, PrivilegeChecker)
  - Knowledge of System.CommandLine usage in the project

- **Windows Search Components**:
  - Windows Search service installed (typically present by default)
  - SearchAPI.dll located in `C:\Windows\System32\`

## Quick Start (5 Minutes)

### 1. Verify Your Environment

```powershell
# Check Windows Search service status
Get-Service WSearch

# Verify SearchAPI.dll exists
Test-Path "$env:SystemRoot\System32\SearchAPI.dll"

# Check if COM API is registered (optional)
$clsid = "{7D096C5F-AC08-4F1F-BEB7-5C22C517CE39}"
Test-Path "HKCR:\CLSID\$clsid"
```

### 2. Review Key Documentation

1. **Read [spec.md](./spec.md)** - Understand functional requirements
2. **Read [research.md](./research.md)** - Review technical decisions
3. **Read [data-model.md](./data-model.md)** - Understand domain model
4. **Read [contracts/cli-contract.md](./contracts/cli-contract.md)** - CLI behavior

### 3. Understand Integration Points

The feature integrates with:
- `Program.cs` - Startup COM detection
- `Services/PrivilegeChecker.cs` - Admin privilege detection
- `Infrastructure/WindowsSearchInterop.cs` - COM validation
- `Utilities/ConsoleFormatter.cs` - User messages

## Development Workflow

### Phase 1: COM Detection (Batch 1)

**Goal**: Implement detection without registration

**Files to Create/Modify** (4 files):
1. `Services/COMRegistrationDetector.cs` - NEW
2. `Services/COMRegistrationDetectorTests.cs` - NEW (unit tests)
3. `Infrastructure/WindowsSearchInterop.cs` - MODIFY (add validation)
4. `Infrastructure/WindowsSearchInteropTests.cs` - MODIFY (add tests)

**Implementation Steps**:

1. **Create COMRegistrationDetector**:
   ```csharp
   public class COMRegistrationDetector : ICOMRegistrationDetector
   {
       private const string SearchManagerCLSID = "{7D096C5F-AC08-4F1F-BEB7-5C22C517CE39}";
       
       public COMRegistrationStatus GetRegistrationStatus()
       {
           // Check CLSID exists
           // Check DLL path and file existence
           // Return status object
       }
   }
   ```

2. **Write Unit Tests First** (TDD):
   ```csharp
   [Test]
   public void GetRegistrationStatus_WhenCLSIDExists_ReturnsRegistered()
   {
       // Arrange: Mock registry accessor
       // Act: Call GetRegistrationStatus
       // Assert: IsRegistered == true
   }
   ```

3. **Run Tests**:
   ```powershell
   dotnet test --filter "FullyQualifiedName~COMRegistrationDetector"
   ```

4. **Integrate Detection** (don't attempt registration yet):
   ```csharp
   // In Program.cs Main()
   var detector = new COMRegistrationDetector();
   var status = detector.GetRegistrationStatus();
   if (!status.IsRegistered)
   {
       Console.WriteLine("COM API not registered (detection only)");
       // Exit for now - registration in next batch
   }
   ```

**Expected Outcome**: Tool detects COM registration state and logs it.

---

### Phase 2: Interactive Registration (Batch 2)

**Goal**: Enable interactive registration with privilege checking

**Files to Create/Modify** (5 files):
1. `Services/COMRegistrationService.cs` - NEW
2. `Services/COMRegistrationServiceTests.cs` - NEW (unit tests)
3. `Services/PrivilegeChecker.cs` - MODIFY (enhance for COM context)
4. `Services/PrivilegeCheckerTests.cs` - MODIFY (add tests)
5. `Utilities/ConsoleFormatter.cs` - MODIFY (add COM prompts)

**Implementation Steps**:

1. **Create COMRegistrationService**:
   ```csharp
   public class COMRegistrationService : ICOMRegistrationService
   {
       public async Task<COMRegistrationAttempt> RegisterCOMAsync(RegistrationOptions options)
       {
           // Check privileges
           // Execute regsvr32.exe
           // Validate registration
           // Return attempt with outcome
       }
   }
   ```

2. **Add Interactive Prompt**:
   ```csharp
   public void ShowRegistrationPrompt()
   {
       Console.WriteLine("COM API not registered. Register now? (Y/N/Q)");
       // Handle user input
   }
   ```

3. **Test Manually** (requires admin):
   ```powershell
   # As admin: Should offer to register
   .\WindowsSearchConfigurator.exe list
   
   # As standard user: Should show elevation instructions
   # (Run in non-admin PowerShell)
   .\WindowsSearchConfigurator.exe list
   ```

**Expected Outcome**: Tool offers registration and handles user response.

---

### Phase 3: Non-Interactive Flags (Batch 3)

**Goal**: Support --auto-register-com and --no-register-com flags

**Files to Create/Modify** (3 files):
1. `Program.cs` - MODIFY (add global options)
2. `Services/COMRegistrationService.cs` - MODIFY (handle flags)
3. `ContractTests/COMRegistrationContractTests.cs` - NEW

**Implementation Steps**:

1. **Add Global Options**:
   ```csharp
   var autoRegisterOption = new Option<bool>("--auto-register-com");
   var noRegisterOption = new Option<bool>("--no-register-com");
   rootCommand.AddGlobalOption(autoRegisterOption);
   rootCommand.AddGlobalOption(noRegisterOption);
   ```

2. **Validate Mutual Exclusivity**:
   ```csharp
   if (autoRegister && noRegister)
   {
       Console.WriteLine("ERROR: Conflicting options");
       return 3;
   }
   ```

3. **Test Contract**:
   ```csharp
   [Test]
   public void AutoRegisterFlag_IsRecognized()
   {
       var args = new[] { "--auto-register-com", "list" };
       // Parse and verify
   }
   ```

4. **Test in CI/CD Scenario**:
   ```powershell
   # Auto-register mode (requires admin)
   .\WindowsSearchConfigurator.exe --auto-register-com list
   
   # No-register mode (fail fast)
   .\WindowsSearchConfigurator.exe --no-register-com list
   ```

**Expected Outcome**: Flags control registration behavior without prompts.

---

### Phase 4: Integration Tests & Documentation (Batch 4)

**Goal**: Comprehensive testing and documentation updates

**Files to Create/Modify** (4 files):
1. `IntegrationTests/COMRegistrationIntegrationTests.cs` - NEW
2. `README.md` - MODIFY (add COM troubleshooting section)
3. `specs/002-com-api-registration/quickstart.md` - THIS FILE
4. `specs/002-com-api-registration/tasks.md` - CREATED BY /speckit.tasks

**Implementation Steps**:

1. **Write Integration Tests**:
   ```csharp
   [TestFixture]
   [RequiresAdmin]
   public class COMRegistrationIntegrationTests
   {
       [Test]
       public async Task EndToEnd_RegisterAndValidate()
       {
           // Unregister COM (test setup)
           // Run detection - should be not registered
           // Run registration
           // Validate success
           // Re-register to restore state
       }
   }
   ```

2. **Update README.md**:
   - Add "COM API Troubleshooting" section
   - Document --auto-register-com and --no-register-com flags
   - Include manual registration instructions

3. **Run Full Test Suite**:
   ```powershell
   # All tests
   dotnet test
   
   # Only COM-related tests
   dotnet test --filter "FullyQualifiedName~COMRegistration"
   ```

**Expected Outcome**: Complete test coverage and user documentation.

---

## Testing Strategies

### Unit Testing (No Admin Required)

**Goal**: Test business logic without Windows dependencies

**Approach**: Mock registry access and process execution

**Example**:
```csharp
[Test]
public void RegisterCOM_WhenNotAdmin_ReturnsInsufficientPrivileges()
{
    var mockPrivilegeChecker = new Mock<IPrivilegeChecker>();
    mockPrivilegeChecker.Setup(p => p.IsAdministrator()).Returns(false);
    
    var service = new COMRegistrationService(mockPrivilegeChecker.Object);
    var result = await service.RegisterCOMAsync(new RegistrationOptions());
    
    Assert.That(result.Outcome, Is.EqualTo(RegistrationOutcome.InsufficientPrivileges));
}
```

**Run Command**:
```powershell
dotnet test --filter "FullyQualifiedName~UnitTests"
```

---

### Integration Testing (Admin Required)

**Goal**: Test actual COM registration on real Windows system

**Setup**:
1. Create VM snapshot for rollback
2. Ensure SearchAPI.dll exists
3. Run tests as Administrator

**Example**:
```csharp
[TestFixture]
[RequiresAdmin]
public class COMRegistrationIntegrationTests
{
    [SetUp]
    public void Setup()
    {
        // Unregister COM to start from known state
        UnregisterCOM();
    }
    
    [TearDown]
    public void Teardown()
    {
        // Re-register COM to restore system
        RegisterCOM();
    }
    
    [Test]
    public async Task RegisterCOM_SuccessfullyRegistersAndValidates()
    {
        var service = new COMRegistrationService();
        var result = await service.RegisterCOMAsync(new RegistrationOptions());
        
        Assert.That(result.Outcome, Is.EqualTo(RegistrationOutcome.Success));
        Assert.That(result.PostValidation, Is.EqualTo(COMValidationState.Valid));
    }
}
```

**Run Command**:
```powershell
# Must run as Administrator
dotnet test --filter "FullyQualifiedName~IntegrationTests"
```

---

### Contract Testing (No Admin Required)

**Goal**: Verify CLI behavior and exit codes

**Example**:
```csharp
[Test]
public void ConflictingFlags_ReturnsExitCode3()
{
    var args = new[] { "--auto-register-com", "--no-register-com", "list" };
    var exitCode = Program.Main(args).Result;
    
    Assert.That(exitCode, Is.EqualTo(3));
}
```

**Run Command**:
```powershell
dotnet test --filter "FullyQualifiedName~ContractTests"
```

---

### Manual Testing Checklist

**Interactive Mode**:
- [ ] Run as admin when COM not registered → Should offer registration
- [ ] Run as standard user when COM not registered → Should show elevation instructions
- [ ] User selects "Yes" → Should register successfully
- [ ] User selects "No" → Should show manual instructions
- [ ] User selects "Quit" → Should exit cleanly
- [ ] User enters invalid input → Should re-prompt

**Automatic Mode (--auto-register-com)**:
- [ ] Run as admin when COM not registered → Should auto-register
- [ ] Run as standard user when COM not registered → Should show elevation error
- [ ] Run when COM already registered → Should proceed silently

**No Register Mode (--no-register-com)**:
- [ ] Run when COM not registered → Should exit with error
- [ ] Run when COM registered → Should proceed normally

**Edge Cases**:
- [ ] SearchAPI.dll missing → Should show "DLL not found" error
- [ ] CLSID registered but DLL corrupt → Should detect and report
- [ ] Registration succeeds but validation fails → Should show warning
- [ ] regsvr32.exe times out → Should show timeout error

---

## Troubleshooting

### Problem: "COM API not registered" but it should be

**Symptoms**: Tool reports COM not registered, but SearchAPI.dll exists

**Diagnosis**:
```powershell
# Check CLSID in registry
$clsid = "{7D096C5F-AC08-4F1F-BEB7-5C22C517CE39}"
Test-Path "HKCR:\CLSID\$clsid"

# Manually register
regsvr32 "$env:SystemRoot\System32\SearchAPI.dll"
```

**Solutions**:
1. Run regsvr32 manually as Administrator
2. Check if DLL is blocked (right-click → Properties → Unblock)
3. Repair Windows Search installation

---

### Problem: Registration succeeds but validation fails

**Symptoms**: regsvr32 exit code 0, but COM object instantiation fails

**Diagnosis**:
```csharp
// Check if COM object can be created
Type searchManagerType = Type.GetTypeFromCLSID(
    new Guid("7D096C5F-AC08-4F1F-BEB7-5C22C517CE39")
);
object searchManager = Activator.CreateInstance(searchManagerType);
```

**Solutions**:
1. Restart computer (COM registration sometimes requires reboot)
2. Check Windows Search service status: `Get-Service WSearch`
3. Repair or reinstall Windows Search
4. Check Windows Event Viewer for COM errors

---

### Problem: Integration tests fail with "Access Denied"

**Symptoms**: Tests fail with UnauthorizedAccessException or similar

**Solutions**:
1. Run Visual Studio as Administrator
2. Add `[RequiresAdmin]` attribute to tests
3. Check if antivirus is blocking regsvr32.exe
4. Verify SearchAPI.dll has proper permissions

---

### Problem: Unit tests fail due to registry mocking issues

**Symptoms**: Registry-related tests fail or are flaky

**Solutions**:
1. Ensure IRegistryAccessor abstraction is used (not direct Registry class)
2. Mock all registry calls in unit tests
3. Don't access real registry in unit tests
4. Use integration tests for real registry interactions

---

## Best Practices

### Code Organization

1. **Separation of Concerns**:
   - Detection logic → COMRegistrationDetector
   - Registration orchestration → COMRegistrationService
   - User interaction → ConsoleFormatter
   - Privilege checking → PrivilegeChecker

2. **Dependency Injection**:
   - Use interfaces (ICOMRegistrationDetector, ICOMRegistrationService)
   - Constructor injection for testability
   - Mock external dependencies in unit tests

3. **Error Handling**:
   - Never throw exceptions to user (catch and convert to outcome enums)
   - Log all errors to AuditLogger
   - Provide actionable error messages

### Testing

1. **Test First**:
   - Write unit tests before implementation (TDD)
   - Define expected behavior in tests
   - Use tests as living documentation

2. **Test Pyramid**:
   - Many unit tests (fast, isolated, no admin)
   - Some integration tests (slower, real Windows, admin required)
   - Few contract tests (CLI behavior, exit codes)

3. **Test Isolation**:
   - Each test should be independent
   - Use setup/teardown to manage state
   - Don't rely on test execution order

### Documentation

1. **Code Documentation**:
   - XML comments on all public APIs
   - Explain "why" not just "what"
   - Document assumptions and constraints

2. **User Documentation**:
   - Clear error messages with next steps
   - Step-by-step manual instructions
   - Troubleshooting guide in README

3. **Developer Documentation**:
   - This quickstart guide
   - Architecture decisions in research.md
   - Data model in data-model.md

---

## Common Gotchas

1. **Registry Access in Tests**:
   - Don't access real registry in unit tests → Mock it
   - Integration tests can access registry but need admin privileges

2. **COM Registration Requires Reboot Sometimes**:
   - Some systems need restart after regsvr32
   - Validation might fail immediately after registration
   - Consider retrying validation after short delay

3. **Antivirus Interference**:
   - Antivirus may block regsvr32.exe execution
   - Tests may be unreliable on dev machines with aggressive AV
   - Document as known limitation

4. **Windows Search Service State**:
   - COM registration doesn't require service running
   - But validation might fail if service is disabled
   - Check service status in troubleshooting

5. **Path Expansion**:
   - Registry stores paths like `%SystemRoot%\System32\SearchAPI.dll`
   - Must use Environment.ExpandEnvironmentVariables()
   - Don't assume paths are fully qualified

---

## Next Steps

After implementing this feature:

1. **Run Full Test Suite**:
   ```powershell
   dotnet test
   ```

2. **Test on Clean VM**:
   - Spin up fresh Windows 10/11 VM
   - Test both admin and standard user scenarios
   - Verify no unintended side effects

3. **Update Project Documentation**:
   - Add COM troubleshooting to README.md
   - Update CHANGELOG.md
   - Document new command-line flags

4. **Create Pull Request**:
   ```powershell
   gh pr create --title "feat(com): Add COM API registration support" --body "Implements #[issue-number]"
   ```

5. **Request Code Review**:
   - Ensure all Constitution gates pass
   - Verify test coverage
   - Validate documentation completeness

---

## Resources

### Internal Documentation

- [Feature Specification](./spec.md)
- [Implementation Plan](./plan.md)
- [Research Findings](./research.md)
- [Data Model](./data-model.md)
- [CLI Contract](./contracts/cli-contract.md)

### External References

- [COM Registration](https://learn.microsoft.com/en-us/windows/win32/com/registering-com-applications)
- [regsvr32 Command](https://learn.microsoft.com/en-us/windows-server/administration/windows-commands/regsvr32)
- [WindowsIdentity Class](https://learn.microsoft.com/en-us/dotnet/api/system.security.principal.windowsidentity)
- [Process Class](https://learn.microsoft.com/en-us/dotnet/api/system.diagnostics.process)
- [NUnit Testing](https://docs.nunit.org/)
- [Moq Mocking Framework](https://github.com/moq/moq4)

### Windows Search Resources

- [Windows Search Overview](https://learn.microsoft.com/en-us/windows/win32/search/-search-3x-wds-overview)
- [Search Manager (ISearchManager)](https://learn.microsoft.com/en-us/windows/win32/api/searchapi/nn-searchapi-isearchmanager)
- Windows Search COM API CLSID: `{7D096C5F-AC08-4F1F-BEB7-5C22C517CE39}`

---

## Questions?

If you encounter issues not covered in this guide:

1. Check audit log: `%USERPROFILE%\.windowssearchconfigurator\audit.log`
2. Review Windows Event Viewer (COM errors)
3. Consult [research.md](./research.md) for technical decisions
4. Ask in team chat or create GitHub issue
