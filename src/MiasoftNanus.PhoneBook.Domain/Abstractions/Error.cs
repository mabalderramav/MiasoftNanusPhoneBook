namespace MiasoftNanus.PhoneBook.Domain.Abstractions;

/// <summary>
/// Represents an error with a specific code and associated message.
/// This can be used to describe and categorize different types of errors in the application.
/// </summary>
public record Error(
    string Code,
    string Message
)
{
    /// <summary>
    /// Represents an empty or undefined error state.
    /// This error has no code or message, signifying the absence of an error.
    /// </summary>
    public static Error None => new(string.Empty, string.Empty);

    /// <summary>
    /// Represents an error indicating that a value is null where it is not allowed.
    /// This error is typically used to enforce non-null value constraints.
    /// </summary>
    public static Error NullValue => new("Error.NullValue", "The value cannot be null.");
}