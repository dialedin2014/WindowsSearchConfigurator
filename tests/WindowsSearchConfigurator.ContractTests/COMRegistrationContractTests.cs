namespace WindowsSearchConfigurator.ContractTests;

/// <summary>
/// Contract tests for COM registration CLI flags and behavior.
/// </summary>
[TestFixture]
public class COMRegistrationContractTests
{
    [Test]
    public void HelpText_ShouldIncludeCOMRegistrationOptions()
    {
        // Act
        var result = RunCLI("--help");

        // Assert
        Assert.That(result.Output, Does.Contain("--auto-register-com"));
        Assert.That(result.Output, Does.Contain("--no-register-com"));
        Assert.That(result.Output, Does.Contain("Automatically register COM API"));
        Assert.That(result.Output, Does.Contain("Do not attempt COM API registration"));
    }

    [Test]
    public void MutuallyExclusiveFlags_ShouldReturnExitCode3()
    {
        // Act
        var result = RunCLI("--auto-register-com --no-register-com list");

        // Assert
        Assert.That(result.ExitCode, Is.EqualTo(3));
        Assert.That(result.Output, Does.Contain("mutually exclusive"));
        Assert.That(result.Output, Does.Contain("--auto-register-com"));
        Assert.That(result.Output, Does.Contain("--no-register-com"));
    }

    [Test]
    public void AutoRegisterFlag_ShouldBeParsed()
    {
        // Act
        var result = RunCLI("--auto-register-com --help");

        // Assert
        // Should not error on flag parsing
        Assert.That(result.ExitCode, Is.EqualTo(0));
    }

    [Test]
    public void NoRegisterFlag_ShouldBeParsed()
    {
        // Act
        var result = RunCLI("--no-register-com --help");

        // Assert
        // Should not error on flag parsing
        Assert.That(result.ExitCode, Is.EqualTo(0));
    }

    [Test]
    public void BothFlags_WithHelp_ShouldShowMutualExclusivityError()
    {
        // Act
        var result = RunCLI("--auto-register-com --no-register-com --help");

        // Assert
        Assert.That(result.ExitCode, Is.EqualTo(3));
        Assert.That(result.Output, Does.Contain("mutually exclusive"));
    }

    [Test]
    public void VersionFlag_ShouldReturnVersionInfo()
    {
        // Act
        var result = RunCLI("--version");

        // Assert
        Assert.That(result.ExitCode, Is.EqualTo(0));
        Assert.That(result.Output, Does.Contain("Windows Search Configurator"));
        Assert.That(result.Output, Does.Contain("v1.0.0"));
    }

    [Test]
    public void VerboseFlag_ShouldBeCompatibleWithCOMFlags()
    {
        // Act
        var result1 = RunCLI("--verbose --auto-register-com --help");
        var result2 = RunCLI("--verbose --no-register-com --help");

        // Assert
        Assert.That(result1.ExitCode, Is.EqualTo(0));
        Assert.That(result2.ExitCode, Is.EqualTo(0));
    }

    [Test]
    public void GlobalOptions_ShouldBeAvailableForAllCommands()
    {
        // Arrange
        string[] commands = { "list", "add", "remove", "modify", "search-extensions", "configure-depth", "export", "import" };

        foreach (var command in commands)
        {
            // Act
            var helpResult = RunCLI($"{command} --help");

            // Assert
            Assert.That(helpResult.Output, Does.Contain("--auto-register-com"), $"Command '{command}' should show --auto-register-com in help");
            Assert.That(helpResult.Output, Does.Contain("--no-register-com"), $"Command '{command}' should show --no-register-com in help");
        }
    }

    #region Helper Methods

    /// <summary>
    /// Runs the CLI with specified arguments and captures output.
    /// </summary>
    /// <param name="args">Command-line arguments.</param>
    /// <returns>CLI execution result.</returns>
    private CLIResult RunCLI(string args)
    {
        var startInfo = new System.Diagnostics.ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = $"run --project \"{GetProjectPath()}\" -- {args}",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            RedirectStandardInput = true, // Redirect stdin to prevent hanging on prompts
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = System.Diagnostics.Process.Start(startInfo);
        if (process == null)
        {
            throw new InvalidOperationException("Failed to start CLI process");
        }

        // Close stdin immediately to prevent the CLI from waiting for input
        process.StandardInput.Close();

        // Read output asynchronously to avoid deadlocks
        var outputTask = process.StandardOutput.ReadToEndAsync();
        var errorTask = process.StandardError.ReadToEndAsync();

        // Wait for process to exit with timeout (2 seconds should be plenty for these tests)
        if (!process.WaitForExit(2000))
        {
            // Process didn't exit - likely waiting for input despite closed stdin
            process.Kill();
            process.WaitForExit(); // Wait for kill to complete
            
            // Try to get whatever output was produced before we killed it
            string output = string.Empty;
            string error = string.Empty;
            
            if (outputTask.IsCompleted)
            {
                output = outputTask.Result;
            }
            if (errorTask.IsCompleted)
            {
                error = errorTask.Result;
            }
            
            return new CLIResult
            {
                ExitCode = -1, // Indicate timeout/kill
                Output = $"[TIMEOUT: Process killed after 2 seconds]\n{output}{error}"
            };
        }

        // Wait for async reads to complete
        var finalOutput = outputTask.Result;
        var finalError = errorTask.Result;

        return new CLIResult
        {
            ExitCode = process.ExitCode,
            Output = finalOutput + finalError
        };
    }

    /// <summary>
    /// Gets the path to the main project.
    /// </summary>
    /// <returns>Project path.</returns>
    private string GetProjectPath()
    {
        // Navigate from test project to main project
        var testDir = TestContext.CurrentContext.TestDirectory;
        var solutionDir = Directory.GetParent(testDir)?.Parent?.Parent?.Parent?.Parent?.FullName;
        return Path.Combine(solutionDir ?? "", "src", "WindowsSearchConfigurator", "WindowsSearchConfigurator.csproj");
    }

    #endregion

    #region Helper Classes

    private class CLIResult
    {
        public int ExitCode { get; set; }
        public string Output { get; set; } = string.Empty;
    }

    #endregion
}
