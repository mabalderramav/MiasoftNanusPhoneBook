using MiasoftNanus.PhoneBook.Domain.Abstractions;

namespace MiasoftNanus.PhoneBook.Domain.Workstations.Errors;

/// <summary>
/// Provides a collection of predefined errors specific to the Workstation domain.
/// These errors can be used for validation and error handling when working with Workstation entities.
/// </summary>
public static class WorkstationErrors
{
    public static Error WorkstationNotFound =>
        new Error("WorkstationErrors.WorkstationNotFound", "The workstation was not found."
        );

    public static Error WorkstationsNotFound =>
        new Error("WorkstationErrors.WorkstationsNotFound", "The workstations were not found."
        );

    public static Error CodeCannotBeNullOrWhitespace =>
        new Error("WorkstationErrors.CodeCannotBeNullOrWhitespace", "Code cannot be null or whitespace."
        );

    public static Error NameCannotBeNullOrWhitespace =>
        new Error("WorkstationErrors.NameCannotBeNullOrWhitespace", "Name cannot be null or whitespace."
        );

    public static Error WorkAreaIdCannotBeEmpty =>
        new Error("WorkstationErrors.WorkAreaIdCannotBeEmpty", "Work area id cannot be empty."
        );
}
