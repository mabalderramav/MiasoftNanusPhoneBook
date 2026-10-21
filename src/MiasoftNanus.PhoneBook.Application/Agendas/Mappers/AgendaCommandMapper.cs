using MiasoftNanus.PhoneBook.Application.Agendas.UseCases.Commands.CreateAgenda;
using MiasoftNanus.PhoneBook.Application.Agendas.UseCases.Commands.UpdateAgenda;
using MiasoftNanus.PhoneBook.Domain.Agendas.Entities;

namespace MiasoftNanus.PhoneBook.Application.Agendas.Mappers;

/// <summary>
/// Provides mapping from agenda commands to <see cref="AgendaCreationData"/>.
/// </summary>
public static class AgendaCommandMapper
{
    public static AgendaCreationData ToAgendaCreationData(this CreateAgendaCommand command)
    {
        return new AgendaCreationData
        {
            Code = command.Code,
            FullName = command.FullName,
            Email = command.Email,
            Enabled = command.Enabled,
            ImagePath = command.ImagePath,
            DirectLine = command.DirectLine,
            Internal = command.Internal,
            Corporate = command.Corporate,
            DirectAccess = command.DirectAccess,
            WorkstationId = command.WorkstationId,
            LocationId = command.LocationId,
            UserIdCreated = command.UserIdCreated
        };
    }

    public static AgendaCreationData ToAgendaCreationData(this UpdateAgendaCommand command)
    {
        return new AgendaCreationData
        {
            Code = command.Code,
            FullName = command.FullName,
            Email = command.Email,
            Enabled = command.Enabled,
            ImagePath = command.ImagePath,
            DirectLine = command.DirectLine,
            Internal = command.Internal,
            Corporate = command.Corporate,
            DirectAccess = command.DirectAccess,
            WorkstationId = command.WorkstationId,
            LocationId = command.LocationId,
            UserIdCreated = command.UserIdCreated
        };
    }
}
