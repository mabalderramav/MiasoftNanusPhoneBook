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
    public static Error None => new(string.Empty, string.Empty);

    public static Error NullValue => new("Error.NullValue", "The value cannot be null.");

    public static Error CannotSaveChanges => new("Error.CannotSaveChanges",
        "Cannot save changes to the database.");
}