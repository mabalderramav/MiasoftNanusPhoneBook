using MiasoftNanus.PhoneBook.Application.Abstractions.CQRS;
using MiasoftNanus.PhoneBook.Domain.Abstractions;
using MiasoftNanus.PhoneBook.Domain.Agendas.Entities;

namespace MiasoftNanus.PhoneBook.Application.Agendas.UseCases.Commands.CreateAgenda;

/// <summary>
/// Represents a command to create an agenda.
/// </summary>
public record CreateAgendaCommand(
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
