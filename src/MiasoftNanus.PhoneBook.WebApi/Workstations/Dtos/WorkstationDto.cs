namespace MiasoftNanus.PhoneBook.WebApi.Workstations.Dtos;

/// <summary>
/// Data transfer object representing a workstation.
/// </summary>
public record WorkstationDto(
    Guid Id,
    string Code,
    string Name,
    string? Description,
    Guid WorkAreaId
);
