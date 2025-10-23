using System.Text;
using System.Text.Json;
using WindowsSearchConfigurator.Core.Models;

namespace WindowsSearchConfigurator.Utilities;

/// <summary>
/// Provides console output formatting for table, JSON, and CSV formats.
/// </summary>
public class ConsoleFormatter
{
    /// <summary>
    /// Formats data as a table with aligned columns.
    /// </summary>
    /// <typeparam name="T">The type of items to format.</typeparam>
    /// <param name="items">The items to format.</param>
    /// <param name="columns">Column definitions (header, selector).</param>
    /// <returns>The formatted table string.</returns>
    public static string FormatAsTable<T>(IEnumerable<T> items, params (string Header, Func<T, string> Selector)[] columns)
    {
        var itemList = items.ToList();
        if (!itemList.Any())
        {
            return "No data to display.";
        }

        // Calculate column widths
        var columnWidths = new int[columns.Length];
        for (int i = 0; i < columns.Length; i++)
        {
            columnWidths[i] = columns[i].Header.Length;
            foreach (var item in itemList)
            {
                var value = columns[i].Selector(item);
                if (value != null && value.Length > columnWidths[i])
                {
                    columnWidths[i] = value.Length;
                }
            }
        }

        var sb = new StringBuilder();

        // Header row
        for (int i = 0; i < columns.Length; i++)
        {
            sb.Append(columns[i].Header.PadRight(columnWidths[i]));
            if (i < columns.Length - 1)
            {
                sb.Append("  ");
            }
        }
        sb.AppendLine();

        // Separator row
        for (int i = 0; i < columns.Length; i++)
        {
            sb.Append(new string('-', columnWidths[i]));
            if (i < columns.Length - 1)
            {
                sb.Append("  ");
            }
        }
        sb.AppendLine();

        // Data rows
        foreach (var item in itemList)
        {
            for (int i = 0; i < columns.Length; i++)
            {
                var value = columns[i].Selector(item) ?? string.Empty;
                sb.Append(value.PadRight(columnWidths[i]));
                if (i < columns.Length - 1)
                {
                    sb.Append("  ");
                }
            }
            sb.AppendLine();
        }

        return sb.ToString();
    }

    /// <summary>
    /// Formats data as JSON.
    /// </summary>
    /// <typeparam name="T">The type of items to format.</typeparam>
    /// <param name="items">The items to format.</param>
    /// <param name="indent">Whether to indent the JSON for readability.</param>
    /// <returns>The JSON string.</returns>
    public static string FormatAsJson<T>(IEnumerable<T> items, bool indent = true)
    {
        var options = new JsonSerializerOptions
        {
            WriteIndented = indent,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        return JsonSerializer.Serialize(items, options);
    }

    /// <summary>
    /// Formats a single object as JSON.
    /// </summary>
    /// <typeparam name="T">The type of object to format.</typeparam>
    /// <param name="item">The object to format.</param>
    /// <param name="indent">Whether to indent the JSON for readability.</param>
    /// <returns>The JSON string.</returns>
    public static string FormatObjectAsJson<T>(T item, bool indent = true)
    {
        var options = new JsonSerializerOptions
        {
            WriteIndented = indent,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        return JsonSerializer.Serialize(item, options);
    }

    /// <summary>
    /// Formats data as CSV (RFC 4180 compliant).
    /// </summary>
    /// <typeparam name="T">The type of items to format.</typeparam>
    /// <param name="items">The items to format.</param>
    /// <param name="columns">Column definitions (header, selector).</param>
    /// <returns>The CSV string.</returns>
    public static string FormatAsCsv<T>(IEnumerable<T> items, params (string Header, Func<T, string> Selector)[] columns)
    {
        var itemList = items.ToList();
        var sb = new StringBuilder();

        // Header row
        sb.AppendLine(string.Join(",", columns.Select(c => EscapeCsvField(c.Header))));

        // Data rows
        foreach (var item in itemList)
        {
            var values = columns.Select(c =>
            {
                var value = c.Selector(item) ?? string.Empty;
                return EscapeCsvField(value);
            });
            sb.AppendLine(string.Join(",", values));
        }

        return sb.ToString();
    }

    /// <summary>
    /// Escapes a CSV field according to RFC 4180.
    /// </summary>
    /// <param name="field">The field value.</param>
    /// <returns>The escaped field value.</returns>
    private static string EscapeCsvField(string field)
    {
        if (string.IsNullOrEmpty(field))
        {
            return string.Empty;
        }

        // If field contains comma, quote, or newline, it must be quoted
        if (field.Contains(',') || field.Contains('"') || field.Contains('\n') || field.Contains('\r'))
        {
            // Escape quotes by doubling them
            field = field.Replace("\"", "\"\"");
            return $"\"{field}\"";
        }

        return field;
    }

    /// <summary>
    /// Formats a success message with a checkmark.
    /// </summary>
    /// <param name="message">The message to format.</param>
    /// <returns>The formatted success message.</returns>
    public static string FormatSuccess(string message)
    {
        return $"✓ {message}";
    }

    /// <summary>
    /// Formats an error message with an X mark.
    /// </summary>
    /// <param name="message">The message to format.</param>
    /// <returns>The formatted error message.</returns>
    public static string FormatError(string message)
    {
        return $"✗ {message}";
    }

    /// <summary>
    /// Formats a warning message with a warning symbol.
    /// </summary>
    /// <param name="message">The message to format.</param>
    /// <returns>The formatted warning message.</returns>
    public static string FormatWarning(string message)
    {
        return $"⚠ {message}";
    }

    /// <summary>
    /// Formats index rules as a table with Unicode box-drawing characters.
    /// </summary>
    /// <param name="rules">The rules to format.</param>
    public void FormatRulesAsTable(IEnumerable<IndexRule> rules)
    {
        var rulesList = rules.ToList();
        if (rulesList.Count == 0)
        {
            Console.WriteLine("No rules to display.");
            return;
        }

        // Define columns
        var columns = new (string, Func<IndexRule, string>)[]
        {
            ("Path", r => r.Path),
            ("Type", r => r.RuleType.ToString()),
            ("Recursive", r => r.Recursive ? "Yes" : "No"),
            ("Filters", r => r.FileTypeFilters?.Count.ToString() ?? "0"),
            ("Source", r => r.Source.ToString())
        };

        // Calculate column widths
        var columnWidths = new int[columns.Length];
        for (int i = 0; i < columns.Length; i++)
        {
            columnWidths[i] = columns[i].Item1.Length;
            foreach (var rule in rulesList)
            {
                var value = columns[i].Item2(rule);
                if (value != null && value.Length > columnWidths[i])
                {
                    columnWidths[i] = value.Length;
                }
            }
        }

        // Print table with Unicode box-drawing
        var sb = new StringBuilder();

        // Top border
        sb.Append("┌");
        for (int i = 0; i < columns.Length; i++)
        {
            sb.Append(new string('─', columnWidths[i] + 2));
            sb.Append(i < columns.Length - 1 ? "┬" : "┐");
        }
        sb.AppendLine();

        // Header row
        sb.Append("│");
        for (int i = 0; i < columns.Length; i++)
        {
            sb.Append($" {columns[i].Item1.PadRight(columnWidths[i])} │");
        }
        sb.AppendLine();

        // Header separator
        sb.Append("├");
        for (int i = 0; i < columns.Length; i++)
        {
            sb.Append(new string('─', columnWidths[i] + 2));
            sb.Append(i < columns.Length - 1 ? "┼" : "┤");
        }
        sb.AppendLine();

        // Data rows
        foreach (var rule in rulesList)
        {
            sb.Append("│");
            for (int i = 0; i < columns.Length; i++)
            {
                var value = columns[i].Item2(rule) ?? string.Empty;
                sb.Append($" {value.PadRight(columnWidths[i])} │");
            }
            sb.AppendLine();
        }

        // Bottom border
        sb.Append("└");
        for (int i = 0; i < columns.Length; i++)
        {
            sb.Append(new string('─', columnWidths[i] + 2));
            sb.Append(i < columns.Length - 1 ? "┴" : "┘");
        }
        sb.AppendLine();

        Console.Write(sb.ToString());
        Console.WriteLine($"\nTotal rules: {rulesList.Count}");
    }

    /// <summary>
    /// Formats index rules as JSON.
    /// </summary>
    /// <param name="rules">The rules to format.</param>
    public void FormatRulesAsJson(IEnumerable<IndexRule> rules)
    {
        var json = FormatAsJson(rules, indent: true);
        Console.WriteLine(json);
    }

    /// <summary>
    /// Formats index rules as CSV.
    /// </summary>
    /// <param name="rules">The rules to format.</param>
    public void FormatRulesAsCsv(IEnumerable<IndexRule> rules)
    {
        var csv = FormatAsCsv(rules,
            ("Path", r => r.Path),
            ("Type", r => r.RuleType.ToString()),
            ("Recursive", r => r.Recursive ? "Yes" : "No"),
            ("Filters", r => r.FileTypeFilters?.Count.ToString() ?? "0"),
            ("ExcludedSubfolders", r => string.Join(";", r.ExcludedSubfolders ?? Enumerable.Empty<string>())),
            ("Source", r => r.Source.ToString()),
            ("CreatedDate", r => r.CreatedDate.ToString("yyyy-MM-dd HH:mm:ss")),
            ("ModifiedDate", r => r.ModifiedDate.ToString("yyyy-MM-dd HH:mm:ss"))
        );
        Console.Write(csv);
    }

    /// <summary>
    /// Formats file extension settings as a table with Unicode box-drawing characters.
    /// </summary>
    /// <param name="extensions">The extension settings to format.</param>
    public void FormatExtensionsAsTable(IEnumerable<FileExtensionSetting> extensions)
    {
        var extensionsList = extensions.ToList();
        if (extensionsList.Count == 0)
        {
            Console.WriteLine("No extensions to display.");
            return;
        }

        // Define columns
        var columns = new (string, Func<FileExtensionSetting, string>)[]
        {
            ("Extension", e => e.Extension),
            ("Indexing Depth", e => e.IndexingDepth.ToString()),
            ("Default", e => e.IsDefaultSetting ? "Yes" : "No"),
            ("Modified", e => e.ModifiedDate.ToString("yyyy-MM-dd HH:mm"))
        };

        // Calculate column widths
        var columnWidths = new int[columns.Length];
        for (int i = 0; i < columns.Length; i++)
        {
            columnWidths[i] = columns[i].Item1.Length;
            foreach (var ext in extensionsList)
            {
                var value = columns[i].Item2(ext);
                if (value != null && value.Length > columnWidths[i])
                {
                    columnWidths[i] = value.Length;
                }
            }
        }

        // Print table with Unicode box-drawing
        var sb = new StringBuilder();

        // Top border
        sb.Append("┌");
        for (int i = 0; i < columns.Length; i++)
        {
            sb.Append(new string('─', columnWidths[i] + 2));
            sb.Append(i < columns.Length - 1 ? "┬" : "┐");
        }
        sb.AppendLine();

        // Header row
        sb.Append("│");
        for (int i = 0; i < columns.Length; i++)
        {
            sb.Append($" {columns[i].Item1.PadRight(columnWidths[i])} │");
        }
        sb.AppendLine();

        // Header separator
        sb.Append("├");
        for (int i = 0; i < columns.Length; i++)
        {
            sb.Append(new string('─', columnWidths[i] + 2));
            sb.Append(i < columns.Length - 1 ? "┼" : "┤");
        }
        sb.AppendLine();

        // Data rows
        foreach (var ext in extensionsList)
        {
            sb.Append("│");
            for (int i = 0; i < columns.Length; i++)
            {
                var value = columns[i].Item2(ext) ?? string.Empty;
                sb.Append($" {value.PadRight(columnWidths[i])} │");
            }
            sb.AppendLine();
        }

        // Bottom border
        sb.Append("└");
        for (int i = 0; i < columns.Length; i++)
        {
            sb.Append(new string('─', columnWidths[i] + 2));
            sb.Append(i < columns.Length - 1 ? "┴" : "┘");
        }
        sb.AppendLine();

        Console.Write(sb.ToString());
        Console.WriteLine($"\nTotal extensions: {extensionsList.Count}");
    }

    /// <summary>
    /// Formats file extension settings as JSON.
    /// </summary>
    /// <param name="extensions">The extension settings to format.</param>
    public void FormatExtensionsAsJson(IEnumerable<FileExtensionSetting> extensions)
    {
        var json = FormatAsJson(extensions, indent: true);
        Console.WriteLine(json);
    }

    /// <summary>
    /// Formats file extension settings as CSV.
    /// </summary>
    /// <param name="extensions">The extension settings to format.</param>
    public void FormatExtensionsAsCsv(IEnumerable<FileExtensionSetting> extensions)
    {
        var csv = FormatAsCsv(extensions,
            ("Extension", e => e.Extension),
            ("IndexingDepth", e => e.IndexingDepth.ToString()),
            ("IsDefault", e => e.IsDefaultSetting ? "Yes" : "No"),
            ("ModifiedDate", e => e.ModifiedDate.ToString("yyyy-MM-dd HH:mm:ss"))
        );
        Console.Write(csv);
    }

    /// <summary>
    /// Displays an error message when COM API is not registered.
    /// </summary>
    /// <param name="status">The COM registration status.</param>
    public static void ShowCOMNotRegisteredError(COMRegistrationStatus status)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("ERROR: Microsoft Windows Search COM API is not registered.");
        Console.ResetColor();
        Console.WriteLine();
        Console.WriteLine("The Windows Search Configurator requires this API to function. The COM API is typically");
        Console.WriteLine("installed with Windows Search but may not be registered on this system.");
        Console.WriteLine();
        
        // Provide specific details based on status
        if (!status.CLSIDExists)
        {
            Console.WriteLine("Details: CLSID not found in registry.");
        }
        else if (!status.DLLExists)
        {
            Console.WriteLine($"Details: DLL not found at path: {status.DLLPath}");
        }
        else if (status.ValidationState != COMValidationState.Valid)
        {
            Console.WriteLine($"Details: COM object validation failed ({status.ValidationState}).");
        }

        if (!string.IsNullOrEmpty(status.ErrorMessage))
        {
            Console.WriteLine($"Error: {status.ErrorMessage}");
        }
    }

    /// <summary>
    /// Displays COM registration success message.
    /// </summary>
    public static void ShowCOMRegistrationSuccess()
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("SUCCESS: COM API registered successfully.");
        Console.ResetColor();
        Console.WriteLine();
        Console.WriteLine("Validating registration...");
    }

    /// <summary>
    /// Displays COM validation success message.
    /// </summary>
    public static void ShowCOMValidationSuccess()
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("SUCCESS: COM API is functional.");
        Console.ResetColor();
        Console.WriteLine();
        Console.WriteLine("Continuing with your command...");
        Console.WriteLine();
    }

    /// <summary>
    /// Displays COM registration error message after a failed attempt.
    /// </summary>
    /// <param name="attempt">The failed registration attempt.</param>
    public static void ShowCOMRegistrationError(COMRegistrationAttempt attempt)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"ERROR: COM registration failed ({attempt.Outcome})");
        Console.ResetColor();
        Console.WriteLine();

        if (!string.IsNullOrEmpty(attempt.ErrorMessage))
        {
            Console.WriteLine($"Details: {attempt.ErrorMessage}");
            Console.WriteLine();
        }

        if (attempt.ExitCode.HasValue && attempt.ExitCode != 0)
        {
            Console.WriteLine($"Exit Code: {attempt.ExitCode}");
            Console.WriteLine();
        }

        // Provide troubleshooting guidance based on outcome
        Console.WriteLine("Troubleshooting:");
        switch (attempt.Outcome)
        {
            case RegistrationOutcome.InsufficientPrivileges:
                Console.WriteLine("- Run this tool as Administrator (right-click → Run as Administrator)");
                Console.WriteLine("- Or use manual registration with an elevated Command Prompt");
                break;

            case RegistrationOutcome.DLLNotFound:
                Console.WriteLine("- Verify Windows Search is installed");
                Console.WriteLine("- Check if SearchAPI.dll exists in System32");
                Console.WriteLine("- Re-install Windows Search feature if necessary");
                break;

            case RegistrationOutcome.Timeout:
                Console.WriteLine("- Try manual registration with regsvr32");
                Console.WriteLine("- Check if Windows Search service is running");
                break;

            case RegistrationOutcome.ValidationFailed:
                Console.WriteLine("- Registration command completed but COM object cannot be instantiated");
                Console.WriteLine("- Try restarting the Windows Search service");
                Console.WriteLine("- Try manual registration with regsvr32");
                break;

            default:
                Console.WriteLine("- Try manual registration (see manual instructions)");
                Console.WriteLine("- Check Windows Event Viewer for additional details");
                break;
        }
        Console.WriteLine();
    }

    /// <summary>
    /// Writes colored text to the console.
    /// </summary>
    /// <param name="text">The text to write.</param>
    /// <param name="color">The console color to use.</param>
    public static void WriteColored(string text, ConsoleColor color)
    {
        Console.ForegroundColor = color;
        Console.WriteLine(text);
        Console.ResetColor();
    }

    /// <summary>
    /// Prompts the user to choose whether to register the COM API.
    /// </summary>
    /// <returns>The user's choice: 'Y', 'N', or 'Q'.</returns>
    public static char PromptCOMRegistration()
    {
        Console.WriteLine();
        Console.WriteLine("Would you like to attempt automatic registration? (requires administrative privileges)");
        Console.WriteLine();
        Console.WriteLine("Options:");
        Console.WriteLine("  [Y] Yes - Attempt automatic registration now");
        Console.WriteLine("  [N] No  - Show manual registration instructions");
        Console.WriteLine("  [Q] Quit - Exit without registering");
        Console.WriteLine();

        while (true)
        {
            Console.Write("Enter your choice (Y/N/Q): ");
            var input = Console.ReadLine()?.Trim().ToUpperInvariant();

            if (string.IsNullOrEmpty(input))
            {
                Console.WriteLine("Invalid choice. Please enter Y, N, or Q.");
                Console.WriteLine();
                continue;
            }

            var choice = input[0];
            if (choice == 'Y' || choice == 'N' || choice == 'Q')
            {
                return choice;
            }

            Console.WriteLine("Invalid choice. Please enter Y, N, or Q.");
            Console.WriteLine();
        }
    }

    /// <summary>
    /// Displays manual COM registration instructions.
    /// </summary>
    public static void ShowManualCOMRegistrationInstructions()
    {
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("Manual COM API Registration Instructions:");
        Console.ResetColor();
        Console.WriteLine();
        Console.WriteLine("To register the COM API manually:");
        Console.WriteLine();
        Console.WriteLine("  1. Open Command Prompt as Administrator:");
        Console.WriteLine("     - Press Win + X");
        Console.WriteLine("     - Select 'Command Prompt (Admin)' or 'Windows PowerShell (Admin)'");
        Console.WriteLine();
        Console.WriteLine("  2. Run the following command:");
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("     regsvr32 \"%SystemRoot%\\System32\\SearchAPI.dll\"");
        Console.ResetColor();
        Console.WriteLine();
        Console.WriteLine("  3. Wait for confirmation message");
        Console.WriteLine();
        Console.WriteLine("  4. Run WindowsSearchConfigurator again");
        Console.WriteLine();
        Console.WriteLine("If registration fails, verify:");
        Console.WriteLine("  - Windows Search is installed");
        Console.WriteLine("  - SearchAPI.dll exists in System32 folder");
        Console.WriteLine("  - You have administrator privileges");
        Console.WriteLine();
    }

    /// <summary>
    /// Displays elevation instructions when user lacks admin privileges.
    /// </summary>
    public static void ShowElevationInstructions()
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("ERROR: Administrative privileges required for COM API registration.");
        Console.ResetColor();
        Console.WriteLine();
        Console.WriteLine("To register the COM API, you must run WindowsSearchConfigurator as Administrator.");
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("How to run as Administrator:");
        Console.ResetColor();
        Console.WriteLine("  1. Right-click on WindowsSearchConfigurator.exe");
        Console.WriteLine("  2. Select \"Run as administrator\"");
        Console.WriteLine("  3. Try your command again");
        Console.WriteLine();
        Console.WriteLine("Or use this command in an elevated prompt:");
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("  WindowsSearchConfigurator.exe [your command]");
        Console.ResetColor();
        Console.WriteLine();
        Console.WriteLine("Alternatively, you can register manually:");
        Console.WriteLine("  1. Open Command Prompt as Administrator");
        Console.WriteLine("  2. Run: regsvr32 \"%SystemRoot%\\System32\\SearchAPI.dll\"");
        Console.WriteLine("  3. Run WindowsSearchConfigurator again");
        Console.WriteLine();
    }

    /// <summary>
    /// Displays a message indicating registration is in progress.
    /// </summary>
    public static void ShowCOMRegistrationInProgress()
    {
        Console.WriteLine();
        Console.WriteLine("Registering COM API...");
    }
}
