using MiasoftNanus.PhoneBook.Domain.Abstractions;

namespace MiasoftNanus.PhoneBook.Domain.WorkAreas.Errors;

/// <summary>
/// Provides a collection of predefined errors specific to the WorkArea domain.
/// These errors can be used for validation and error handling when working with WorkArea entities.
/// </summary>
public static class WorkAreaErrors
{
    public static Error CodeCannotBeNullOrWhitespace =>
        new Error("WorkAreaErrors.CodeCannotBeNullOrWhitespace", "Code cannot be null or whitespace."
        );
    
    public static Error NameCannotBeNullOrWhitespace =>
        new Error("WorkAreaErrors.NameCannotBeNullOrWhitespace", "Name cannot be null or whitespace."
        );
}