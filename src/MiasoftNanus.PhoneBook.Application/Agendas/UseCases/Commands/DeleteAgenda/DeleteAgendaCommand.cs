using MiasoftNanus.PhoneBook.Application.Abstractions.CQRS;
using MiasoftNanus.PhoneBook.Domain.Abstractions;

namespace MiasoftNanus.PhoneBook.Application.Agendas.UseCases.Commands.DeleteAgenda;

/// <summary>
/// Represents a command to delete an agenda.
/// </summary>
public record DeleteAgendaCommand(Guid Id) : ICommand<Result>;
