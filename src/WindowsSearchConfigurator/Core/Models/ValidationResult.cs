namespace WindowsSearchConfigurator.Core.Models;

/// <summary>
/// Represents the outcome of a validation operation.
/// </summary>
public class ValidationResult
{
    /// <summary>
    /// Gets whether the validation passed.
    /// </summary>
    public bool IsValid { get; init; }

    /// <summary>
    /// Gets the normalized value after validation (if successful).
    /// </summary>
    public string? NormalizedValue { get; init; }

    /// <summary>
    /// Gets the error or warning message (if applicable).
    /// </summary>
    public string? ErrorMessage { get; init; }

    /// <summary>
    /// Gets the severity level of the validation result.
    /// </summary>
    public ValidationSeverity Severity { get; init; }

    /// <summary>
    /// Creates a successful validation result.
    /// </summary>
    /// <param name="normalizedValue">The normalized value after validation.</param>
    /// <returns>A successful <see cref="ValidationResult"/>.</returns>
    public static ValidationResult Success(string normalizedValue) => new()
    {
        IsValid = true,
        NormalizedValue = normalizedValue,
        Severity = ValidationSeverity.None
    };

    /// <summary>
    /// Creates a failed validation result.
    /// </summary>
    /// <param name="error">The error message.</param>
    /// <returns>A failed <see cref="ValidationResult"/>.</returns>
    public static ValidationResult Failure(string error) => new()
    {
        IsValid = false,
        ErrorMessage = error,
        Severity = ValidationSeverity.Error
    };

    /// <summary>
    /// Creates a validation result with a warning.
    /// </summary>
    /// <param name="warning">The warning message.</param>
    /// <param name="normalizedValue">The normalized value after validation.</param>
    /// <returns>A <see cref="ValidationResult"/> with a warning.</returns>
    public static ValidationResult Warning(string warning, string normalizedValue) => new()
    {
        IsValid = true,
        NormalizedValue = normalizedValue,
        ErrorMessage = warning,
        Severity = ValidationSeverity.Warning
    };
}

/// <summary>
/// Specifies the severity level of a validation result.
/// </summary>
public enum ValidationSeverity
{
    /// <summary>
    /// No issues detected.
    /// </summary>
    None = 0,

    /// <summary>
    /// Validation passed with a warning.
    /// </summary>
    Warning = 1,

    /// <summary>
    /// Validation failed with an error.
    /// </summary>
    Error = 2
}
