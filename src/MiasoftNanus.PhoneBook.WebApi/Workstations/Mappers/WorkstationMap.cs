using MiasoftNanus.PhoneBook.Domain.Abstractions;
using MiasoftNanus.PhoneBook.Domain.Workstations.Entities;
using MiasoftNanus.PhoneBook.WebApi.Workstations.Dtos;

namespace MiasoftNanus.PhoneBook.WebApi.Workstations.Mappers;

/// <summary>
/// Provides mapping between workstation entities and DTOs.
/// </summary>
public static class WorkstationMap
{
    public static WorkstationDto ToDto(this Workstation workstation)
    {
        return new WorkstationDto(
            workstation.Id,
            workstation.Code,
            workstation.Name,
            workstation.Description,
            workstation.WorkAreaId
        );
    }

    public static IEnumerable<WorkstationDto> ToDto(this IEnumerable<Workstation> workstations)
    {
        return workstations.Select(workstation => workstation.ToDto());
    }

    public static Result<Workstation> ToEntity(this CreateUpdateWorkstationDto createUpdateWorkstationDto)
    {
        return Workstation.Create(
            createUpdateWorkstationDto.Code,
            createUpdateWorkstationDto.Name,
            createUpdateWorkstationDto.Description,
            createUpdateWorkstationDto.WorkAreaId
        );
    }
}
