namespace WindowsSearchConfigurator.Core.Models;

/// <summary>
/// Represents the outcome of an operation.
/// </summary>
public class OperationResult
{
    /// <summary>
    /// Gets whether the operation succeeded.
    /// </summary>
    public bool Success { get; init; }

    /// <summary>
    /// Gets the success or error message.
    /// </summary>
    public string Message { get; init; } = string.Empty;

    /// <summary>
    /// Gets the error code (if applicable).
    /// </summary>
    public int? ErrorCode { get; init; }

    /// <summary>
    /// Gets the exception that occurred (if any).
    /// </summary>
    public Exception? Exception { get; init; }

    /// <summary>
    /// Creates a successful operation result.
    /// </summary>
    /// <param name="message">Optional success message.</param>
    /// <returns>A successful <see cref="OperationResult"/>.</returns>
    public static OperationResult Ok(string message = "Operation completed successfully") => new()
    {
        Success = true,
        Message = message
    };

    /// <summary>
    /// Creates a failed operation result.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="errorCode">Optional error code.</param>
    /// <param name="exception">Optional exception.</param>
    /// <returns>A failed <see cref="OperationResult"/>.</returns>
    public static OperationResult Fail(string message, int? errorCode = null, Exception? exception = null) => new()
    {
        Success = false,
        Message = message,
        ErrorCode = errorCode,
        Exception = exception
    };
}

/// <summary>
/// Represents the outcome of an operation that returns a value.
/// </summary>
/// <typeparam name="T">The type of the result value.</typeparam>
public class OperationResult<T> : OperationResult
{
    /// <summary>
    /// Gets the result value (if successful).
    /// </summary>
    public T? Value { get; init; }

    /// <summary>
    /// Creates a successful operation result with a value.
    /// </summary>
    /// <param name="value">The result value.</param>
    /// <param name="message">Optional success message.</param>
    /// <returns>A successful <see cref="OperationResult{T}"/>.</returns>
    public static OperationResult<T> Ok(T value, string message = "Operation completed successfully") => new()
    {
        Success = true,
        Value = value,
        Message = message
    };

    /// <summary>
    /// Creates a failed operation result without a value.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="errorCode">Optional error code.</param>
    /// <param name="exception">Optional exception.</param>
    /// <returns>A failed <see cref="OperationResult{T}"/>.</returns>
    public new static OperationResult<T> Fail(string message, int? errorCode = null, Exception? exception = null) => new()
    {
        Success = false,
        Message = message,
        ErrorCode = errorCode,
        Exception = exception
    };
}
