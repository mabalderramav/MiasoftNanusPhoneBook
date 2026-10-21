using MiasoftNanus.PhoneBook.Domain.Abstractions;
using MiasoftNanus.PhoneBook.Domain.WorkAreas.Entities;
using MiasoftNanus.PhoneBook.WebApi.WorkAreas.Dtos;

namespace MiasoftNanus.PhoneBook.WebApi.WorkAreas.Mappers;

/// <summary>
/// Provides mapping between work area entities and DTOs.
/// </summary>
public static class WorkAreaMap
{
    public static WorkAreaDto ToDto(this WorkArea workArea)
    {
        return new WorkAreaDto(
            workArea.Id,
            workArea.Code,
            workArea.Name,
            workArea.Description
        );
    }

    public static IEnumerable<WorkAreaDto> ToDto(this IEnumerable<WorkArea> workAreas)
    {
        return workAreas.Select(workArea => workArea.ToDto());
    }

    public static Result<WorkArea> ToEntity(this CreateUpdateWorkAreaDto createUpdateWorkAreaDto)
    {
        return WorkArea.Create(
            createUpdateWorkAreaDto.Code,
            createUpdateWorkAreaDto.Name,
            createUpdateWorkAreaDto.Description
        );
    }
}
