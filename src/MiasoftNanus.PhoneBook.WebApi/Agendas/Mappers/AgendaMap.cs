using MiasoftNanus.PhoneBook.Domain.Abstractions;
using MiasoftNanus.PhoneBook.Domain.Agendas.Entities;
using MiasoftNanus.PhoneBook.WebApi.Agendas.Dtos;

namespace MiasoftNanus.PhoneBook.WebApi.Agendas.Mappers;

/// <summary>
/// Provides mapping between agenda entities and DTOs.
/// </summary>
public static class AgendaMap
{
    public static AgendaDto ToDto(this Agenda agenda)
    {
        return new AgendaDto(
            agenda.Id,
            agenda.Code,
            agenda.FullName,
            agenda.Email!.Value,
            agenda.Enabled,
            agenda.ImagePath,
            agenda.DirectLine,
            agenda.Internal,
            agenda.Corporate,
            agenda.DirectAccess,
            agenda.WorkstationId,
            agenda.LocationId,
            agenda.UserIdCreated
        );
    }

    public static IEnumerable<AgendaDto> ToDto(this IEnumerable<Agenda> agendas)
    {
        return agendas.Select(agenda => agenda.ToDto());
    }

    public static Result<Agenda> ToEntity(this CreateUpdateAgendaDto createUpdateAgendaDto)
    {
        return Agenda.Create(new AgendaCreationData
        {
            Code = createUpdateAgendaDto.Code,
            FullName = createUpdateAgendaDto.FullName,
            Email = createUpdateAgendaDto.Email,
            Enabled = createUpdateAgendaDto.Enabled,
            ImagePath = createUpdateAgendaDto.ImagePath,
            DirectLine = createUpdateAgendaDto.DirectLine,
            Internal = createUpdateAgendaDto.Internal,
            Corporate = createUpdateAgendaDto.Corporate,
            DirectAccess = createUpdateAgendaDto.DirectAccess,
            WorkstationId = createUpdateAgendaDto.WorkstationId,
            LocationId = createUpdateAgendaDto.LocationId,
            UserIdCreated = createUpdateAgendaDto.UserIdCreated
        });
    }
}
