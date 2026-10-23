using MiasoftNanus.PhoneBook.Application.Abstractions.CQRS;
using MiasoftNanus.PhoneBook.Domain.Abstractions;
using MiasoftNanus.PhoneBook.Domain.Agendas.Entities;
using MiasoftNanus.PhoneBook.Domain.Agendas.Errors;
using MiasoftNanus.PhoneBook.Domain.Agendas.Repositories;

namespace MiasoftNanus.PhoneBook.Application.Agendas.UseCases.Queries.GetAgendaById;

/// <summary>
/// Retrieves an agenda by identifier from the repository.
/// </summary>
public class GetAgendaByIdQueryHandler(IAgendaRepository agendaRepository) :
    IQueryHandler<GetAgendaByIdQuery, Result<Agenda>>
{
    public async Task<Result<Agenda>> HandleAsync(
        GetAgendaByIdQuery query,
        CancellationToken cancellationToken = default)
    {
        var agenda = await agendaRepository.GetByIdWithDetailsAsync(query.Id, cancellationToken);
        return agenda is not null
            ? Result.Success(agenda)
            : Result.Failure<Agenda>(AgendaErrors.AgendaNotFound);
    }
}
