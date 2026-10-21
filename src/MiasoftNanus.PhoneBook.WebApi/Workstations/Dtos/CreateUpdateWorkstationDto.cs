namespace MiasoftNanus.PhoneBook.WebApi.Workstations.Dtos;

/// <summary>
/// Data transfer object used for creating or updating a workstation.
/// </summary>
public record CreateUpdateWorkstationDto(
    string Code,
    string Name,
    string? Description,
    Guid WorkAreaId
);
