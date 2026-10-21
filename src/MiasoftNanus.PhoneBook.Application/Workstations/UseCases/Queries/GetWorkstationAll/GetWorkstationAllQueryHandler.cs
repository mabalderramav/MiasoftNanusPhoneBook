using MiasoftNanus.PhoneBook.Application.Abstractions.CQRS;
using MiasoftNanus.PhoneBook.Domain.Abstractions;
using MiasoftNanus.PhoneBook.Domain.Workstations.Entities;
using MiasoftNanus.PhoneBook.Domain.Workstations.Errors;
using MiasoftNanus.PhoneBook.Domain.Workstations.Repositories;

namespace MiasoftNanus.PhoneBook.Application.Workstations.UseCases.Queries.GetWorkstationAll;

/// <summary>
/// Retrieves all workstations from the repository.
/// </summary>
public class GetWorkstationAllQueryHandler(IWorkstationRepository workstationRepository) :
    IQueryHandler<GetWorkstationAllQuery, Result<ICollection<Workstation>>>
{
    public async Task<Result<ICollection<Workstation>>> HandleAsync(
        GetWorkstationAllQuery query,
        CancellationToken cancellationToken = default)
    {
        var workstations = await workstationRepository.GetAllAsync(cancellationToken);
        return workstations.Count > 0
            ? Result.Success(workstations)
            : Result.Failure<ICollection<Workstation>>(WorkstationErrors.WorkstationsNotFound);
    }
}
