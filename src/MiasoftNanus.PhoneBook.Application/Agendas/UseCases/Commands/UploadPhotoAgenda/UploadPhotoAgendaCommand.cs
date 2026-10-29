using MiasoftNanus.PhoneBook.Application.Abstractions.CQRS;
using MiasoftNanus.PhoneBook.Domain.Abstractions;
using MiasoftNanus.PhoneBook.Domain.Agendas.Entities;

namespace MiasoftNanus.PhoneBook.Application.Agendas.UseCases.Commands.UploadPhotoAgenda;

/// <summary>
/// Represents a command to upload the photo of an agenda.
/// </summary>
public sealed record UploadPhotoAgendaCommand(
    Guid Id,
    Stream Content,
    string FileName,
    long Length
) : ICommand<Result<Agenda>>;
