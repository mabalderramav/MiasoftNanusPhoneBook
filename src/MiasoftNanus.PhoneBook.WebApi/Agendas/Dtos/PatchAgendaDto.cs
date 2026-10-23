namespace MiasoftNanus.PhoneBook.WebApi.Agendas.Dtos;

/// <summary>
/// Data transfer object used for partially updating an agenda (JSON Merge Patch). A missing or <c>null</c> member
/// keeps the current value; an empty string clears an optional phone / image member.
/// </summary>
public record PatchAgendaDto(
    string? Code,
    string? FullName,
    string? Email,
    bool? Enabled,
    string? ImagePath,
    string? DirectLine,
    string? Internal,
    string? Corporate,
    string? DirectAccess,
    Guid? WorkstationId,
    Guid? LocationId
);
