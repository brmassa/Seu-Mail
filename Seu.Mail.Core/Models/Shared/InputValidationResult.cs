namespace Seu.Mail.Core.Models.Shared;

/// <summary>
/// Represents the result of an input validation operation.
/// </summary>
public class InputValidationResult
{
    /// <summary>
    /// Gets a value indicating whether the input is valid.
    /// </summary>
    public bool IsValid { get; }

    /// <summary>
    /// Gets the error message associated with the validation result, if any.
    /// </summary>
    public string ErrorMessage { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="InputValidationResult"/> class.
    /// </summary>
    /// <param name="isValid">Indicates whether the input is valid.</param>
    /// <param name="errorMessage">The error message, if any.</param>
    public InputValidationResult(bool isValid, string errorMessage = "")
    {
        IsValid = isValid;
        ErrorMessage = errorMessage ?? string.Empty;
    }
}
