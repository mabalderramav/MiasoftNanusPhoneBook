namespace MiasoftNanus.PhoneBook.WebApi.Agendas.Dtos;

/// <summary>
/// Data transfer object representing an agenda.
/// </summary>
public record AgendaDto(
    Guid Id,
    string Code,
    string FullName,
    string Email,
    bool Enabled,
    string? ImagePath,
    string? DirectLine,
    string? Internal,
    string? Corporate,
    string? DirectAccess,
    Guid WorkstationId,
    Guid LocationId,
    Guid UserIdCreated
);
