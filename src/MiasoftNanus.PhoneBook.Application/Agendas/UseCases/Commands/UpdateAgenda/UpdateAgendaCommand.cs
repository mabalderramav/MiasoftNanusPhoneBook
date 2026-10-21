using MiasoftNanus.PhoneBook.Application.Abstractions.CQRS;
using MiasoftNanus.PhoneBook.Domain.Abstractions;
using MiasoftNanus.PhoneBook.Domain.Agendas.Entities;

namespace MiasoftNanus.PhoneBook.Application.Agendas.UseCases.Commands.UpdateAgenda;

/// <summary>
/// Represents a command to update an agenda. <see cref="UserIdCreated"/> is ignored on update.
/// </summary>
public record UpdateAgendaCommand(
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
) : ICommand<Result<Agenda>>;
