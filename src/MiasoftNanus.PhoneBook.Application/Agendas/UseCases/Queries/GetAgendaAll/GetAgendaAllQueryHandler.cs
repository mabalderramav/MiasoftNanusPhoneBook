using MiasoftNanus.PhoneBook.Application.Abstractions.CQRS;
using MiasoftNanus.PhoneBook.Domain.Abstractions;
using MiasoftNanus.PhoneBook.Domain.Agendas.Entities;
using MiasoftNanus.PhoneBook.Domain.Agendas.Errors;
using MiasoftNanus.PhoneBook.Domain.Agendas.Repositories;

namespace MiasoftNanus.PhoneBook.Application.Agendas.UseCases.Queries.GetAgendaAll;

/// <summary>
/// Retrieves all agendas from the repository.
/// </summary>
public class GetAgendaAllQueryHandler(IAgendaRepository agendaRepository) :
    IQueryHandler<GetAgendaAllQuery, Result<ICollection<Agenda>>>
{
    public async Task<Result<ICollection<Agenda>>> HandleAsync(
        GetAgendaAllQuery query,
        CancellationToken cancellationToken = default)
    {
        var agendas = await agendaRepository.GetAllAsync(cancellationToken);
        return agendas.Count > 0
            ? Result.Success(agendas)
            : Result.Failure<ICollection<Agenda>>(AgendaErrors.AgendasNotFound);
    }
}
