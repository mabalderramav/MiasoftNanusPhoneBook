using MiasoftNanus.PhoneBook.Domain.Abstractions;

namespace MiasoftNanus.PhoneBook.Domain.Agendas.Errors;

public static class AgendaErrors
{
    public static Error CodeCannotBeNullOrWhitespace =>
        new Error("AgendaErrors.CodeCannotBeNullOrWhitespace", "Code cannot be null or whitespace."
        );

    public static Error FullNameCannotBeNullOrWhitespace =>
        new Error("AgendaErrors.FullNameCannotBeNullOrWhitespace",
            "FullName cannot be null or whitespace."
        );
}