using MiasoftNanus.PhoneBook.WebApi.Locations.Dtos;
using MiasoftNanus.PhoneBook.WebApi.Users.Dtos;
using MiasoftNanus.PhoneBook.WebApi.Workstations.Dtos;

namespace MiasoftNanus.PhoneBook.WebApi.Agendas.Dtos;

/// <summary>
/// Data transfer object representing an agenda. The workstation, location and creator user are embedded next to
/// their identifiers.
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
    Guid UserIdCreated,
    WorkstationDto? Workstation,
    LocationDto? Location,
    UserSummaryDto? UserCreated
);
