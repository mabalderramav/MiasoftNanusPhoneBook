using MiasoftNanus.PhoneBook.Domain.Abstractions;

namespace MiasoftNanus.PhoneBook.Domain.Workstations.Errors;

public static class WorkstationErrors
{
    public static Error CodeCannotBeNullOrWhitespace =>
        new Error("WorkAreaErrors.CodeCannotBeNullOrWhitespace", "Code cannot be null or whitespace."
        );

    public static Error NameCannotBeNullOrWhitespace =>
        new Error("WorkAreaErrors.NameCannotBeNullOrWhitespace", "Name cannot be null or whitespace."
        );
}