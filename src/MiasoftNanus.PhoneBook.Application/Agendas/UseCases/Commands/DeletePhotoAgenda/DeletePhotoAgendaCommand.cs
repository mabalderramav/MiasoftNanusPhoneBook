using MiasoftNanus.PhoneBook.Application.Abstractions.CQRS;
using MiasoftNanus.PhoneBook.Domain.Abstractions;

namespace MiasoftNanus.PhoneBook.Application.Agendas.UseCases.Commands.DeletePhotoAgenda;

/// <summary>
/// Represents a command to delete the photo of an agenda.
/// </summary>
public sealed record DeletePhotoAgendaCommand(Guid Id) : ICommand<Result>;
