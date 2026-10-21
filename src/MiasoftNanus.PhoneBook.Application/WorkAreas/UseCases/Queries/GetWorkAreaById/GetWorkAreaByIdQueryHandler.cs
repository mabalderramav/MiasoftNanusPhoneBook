using MiasoftNanus.PhoneBook.Application.Abstractions.CQRS;
using MiasoftNanus.PhoneBook.Domain.Abstractions;
using MiasoftNanus.PhoneBook.Domain.WorkAreas.Entities;
using MiasoftNanus.PhoneBook.Domain.WorkAreas.Errors;
using MiasoftNanus.PhoneBook.Domain.WorkAreas.Repositories;

namespace MiasoftNanus.PhoneBook.Application.WorkAreas.UseCases.Queries.GetWorkAreaById;

/// <summary>
/// Retrieves a work area by identifier from the repository.
/// </summary>
public class GetWorkAreaByIdQueryHandler(IWorkAreaRepository workAreaRepository) :
    IQueryHandler<GetWorkAreaByIdQuery, Result<WorkArea>>
{
    public async Task<Result<WorkArea>> HandleAsync(
        GetWorkAreaByIdQuery query,
        CancellationToken cancellationToken = default)
    {
        var workArea = await workAreaRepository.GetByIdAsync(query.Id, cancellationToken);
        return workArea is not null
            ? Result.Success(workArea)
            : Result.Failure<WorkArea>(WorkAreaErrors.WorkAreaNotFound);
    }
}
