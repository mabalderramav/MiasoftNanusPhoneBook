using MiasoftNanus.PhoneBook.WebApi.Locations.Dtos;
using MiasoftNanus.PhoneBook.WebApi.Users.Dtos;
using MiasoftNanus.PhoneBook.WebApi.Workstations.Dtos;

namespace MiasoftNanus.PhoneBook.WebApi.Agendas.Dtos;

/// <summary>
/// Data transfer object representing an agenda, with its workstation, location and creator user embedded.
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
    WorkstationDto Workstation,
    LocationDto Location,
    UserSummaryDto UserCreated
);
