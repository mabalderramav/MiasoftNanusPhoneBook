using MiasoftNanus.PhoneBook.Domain.Abstractions;

namespace MiasoftNanus.PhoneBook.Domain.Workstations.Errors;

public static class WorkstationErrors
{
    public static Error CodeCannotBeNullOrWhitespace =>
        new Error("WorkstationErrors.CodeCannotBeNullOrWhitespace", "Code cannot be null or whitespace."
        );

    public static Error NameCannotBeNullOrWhitespace =>
        new Error("WorkstationErrors.NameCannotBeNullOrWhitespace", "Name cannot be null or whitespace."
        );
}