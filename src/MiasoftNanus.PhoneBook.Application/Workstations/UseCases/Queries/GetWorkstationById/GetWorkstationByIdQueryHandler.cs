using MiasoftNanus.PhoneBook.Application.Abstractions.CQRS;
using MiasoftNanus.PhoneBook.Domain.Abstractions;
using MiasoftNanus.PhoneBook.Domain.Workstations.Entities;
using MiasoftNanus.PhoneBook.Domain.Workstations.Errors;
using MiasoftNanus.PhoneBook.Domain.Workstations.Repositories;

namespace MiasoftNanus.PhoneBook.Application.Workstations.UseCases.Queries.GetWorkstationById;

/// <summary>
/// Retrieves a workstation by identifier from the repository.
/// </summary>
public class GetWorkstationByIdQueryHandler(IWorkstationRepository workstationRepository) :
    IQueryHandler<GetWorkstationByIdQuery, Result<Workstation>>
{
    public async Task<Result<Workstation>> HandleAsync(
        GetWorkstationByIdQuery query,
        CancellationToken cancellationToken = default)
    {
        var workstation = await workstationRepository.GetByIdAsync(query.Id, cancellationToken);
        return workstation is not null
            ? Result.Success(workstation)
            : Result.Failure<Workstation>(WorkstationErrors.WorkstationNotFound);
    }
}
