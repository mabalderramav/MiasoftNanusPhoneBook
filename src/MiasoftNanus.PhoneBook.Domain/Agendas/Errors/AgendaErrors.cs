using MiasoftNanus.PhoneBook.Domain.Abstractions;

namespace MiasoftNanus.PhoneBook.Domain.Agendas.Errors;

/// <summary>
/// Provides a collection of predefined errors specific to the Agenda domain.
/// These errors can be used for validation and error handling when working with Agenda entities.
/// </summary>
public static class AgendaErrors
{
    public static Error AgendaNotFound =>
        new Error("AgendaErrors.AgendaNotFound", "The agenda was not found."
        );

    public static Error AgendasNotFound =>
        new Error("AgendaErrors.AgendasNotFound", "The agendas were not found."
        );

    public static Error CodeCannotBeNullOrWhitespace =>
        new Error("AgendaErrors.CodeCannotBeNullOrWhitespace", "Code cannot be null or whitespace."
        );

    public static Error FullNameCannotBeNullOrWhitespace =>
        new Error("AgendaErrors.FullNameCannotBeNullOrWhitespace",
            "FullName cannot be null or whitespace."
        );

    public static Error WorkstationIdCannotBeEmpty =>
        new Error("AgendaErrors.WorkstationIdCannotBeEmpty", "Workstation id cannot be empty."
        );

    public static Error LocationIdCannotBeEmpty =>
        new Error("AgendaErrors.LocationIdCannotBeEmpty", "Location id cannot be empty."
        );

    public static Error UserIdCreatedCannotBeEmpty =>
        new Error("AgendaErrors.UserIdCreatedCannotBeEmpty", "User id created cannot be empty."
        );
}
