using MiasoftNanus.PhoneBook.Application.Abstractions.CQRS;
using MiasoftNanus.PhoneBook.Domain.Abstractions;
using MiasoftNanus.PhoneBook.Domain.WorkAreas.Entities;
using MiasoftNanus.PhoneBook.Domain.WorkAreas.Errors;
using MiasoftNanus.PhoneBook.Domain.WorkAreas.Repositories;

namespace MiasoftNanus.PhoneBook.Application.WorkAreas.UseCases.Queries.GetWorkAreaAll;

/// <summary>
/// Retrieves all work areas from the repository.
/// </summary>
public class GetWorkAreaAllQueryHandler(IWorkAreaRepository workAreaRepository) :
    IQueryHandler<GetWorkAreaAllQuery, Result<ICollection<WorkArea>>>
{
    public async Task<Result<ICollection<WorkArea>>> HandleAsync(
        GetWorkAreaAllQuery query,
        CancellationToken cancellationToken = default)
    {
        var workAreas = await workAreaRepository.GetAllAsync(cancellationToken);
        return workAreas.Count > 0
            ? Result.Success(workAreas)
            : Result.Failure<ICollection<WorkArea>>(WorkAreaErrors.WorkAreasNotFound);
    }
}
