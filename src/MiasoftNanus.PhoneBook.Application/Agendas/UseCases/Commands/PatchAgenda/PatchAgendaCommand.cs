using MiasoftNanus.PhoneBook.Application.Abstractions.CQRS;
using MiasoftNanus.PhoneBook.Domain.Abstractions;
using MiasoftNanus.PhoneBook.Domain.Agendas.Entities;

namespace MiasoftNanus.PhoneBook.Application.Agendas.UseCases.Commands.PatchAgenda;

/// <summary>
/// Represents a command to partially update an agenda.
/// </summary>
public sealed record PatchAgendaCommand(Guid Id, AgendaPatchData Data) : ICommand<Result<Agenda>>;
