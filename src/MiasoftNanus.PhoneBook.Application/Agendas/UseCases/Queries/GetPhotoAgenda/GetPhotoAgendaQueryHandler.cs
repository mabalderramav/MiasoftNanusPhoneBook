using MiasoftNanus.PhoneBook.Application.Abstractions.CQRS;
using MiasoftNanus.PhoneBook.Application.Abstractions.Storage;
using MiasoftNanus.PhoneBook.Domain.Abstractions;
using MiasoftNanus.PhoneBook.Domain.Agendas.Entities;
using MiasoftNanus.PhoneBook.Domain.Agendas.Errors;
using MiasoftNanus.PhoneBook.Domain.Agendas.Repositories;

namespace MiasoftNanus.PhoneBook.Application.Agendas.UseCases.Queries.GetPhotoAgenda;

/// <summary>
/// Opens the stored photo of an agenda.
/// </summary>
public class GetPhotoAgendaQueryHandler(
    IAgendaRepository agendaRepository,
    IFileStorage fileStorage
) : IQueryHandler<GetPhotoAgendaQuery, Result<AgendaPhoto>>
{
    public async Task<Result<AgendaPhoto>> HandleAsync(
        GetPhotoAgendaQuery query,
        CancellationToken cancellationToken = default)
    {
        // Only the image path is needed: no details (workstation, location, user) are loaded.
        var agenda = await agendaRepository.GetByIdAsync(query.Id, cancellationToken);
        if (agenda is null)
            return Result.Failure<AgendaPhoto>(AgendaErrors.AgendaNotFound);

        if (string.IsNullOrWhiteSpace(agenda.ImagePath))
            return Result.Failure<AgendaPhoto>(AgendaErrors.PhotoNotFound);
        
        var content = await fileStorage.OpenReadAsync(agenda.ImagePath, cancellationToken);
        if (content is null)
            return Result.Failure<AgendaPhoto>(AgendaErrors.PhotoNotFound);

        return Result.Success(new AgendaPhoto(content, Path.GetFileName(agenda.ImagePath)));
    }
}
