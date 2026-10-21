using MiasoftNanus.PhoneBook.Application.Abstractions.CQRS;
using MiasoftNanus.PhoneBook.Domain.Abstractions;
using MiasoftNanus.PhoneBook.Domain.WorkAreas.Errors;
using MiasoftNanus.PhoneBook.Domain.WorkAreas.Repositories;

namespace MiasoftNanus.PhoneBook.Application.WorkAreas.UseCases.Commands.DeleteWorkArea;

/// <summary>
/// Deletes and persists removal of an existing work area.
/// </summary>
public sealed class DeleteWorkAreaCommandHandler(
    IWorkAreaRepository workAreaRepository,
    IUnitOfWork unitOfWork
) : ICommandHandler<DeleteWorkAreaCommand, Result>
{
    public async Task<Result> HandleAsync(
        DeleteWorkAreaCommand command,
        CancellationToken cancellationToken = default)
    {
        var workArea = await workAreaRepository.GetByIdAsync(command.Id, cancellationToken);
        if (workArea is null)
            return Result.Failure(WorkAreaErrors.WorkAreaNotFound);

        workAreaRepository.Remove(workArea);
        var result = await unitOfWork.SaveChangesAsync(cancellationToken);

        return result > 0 ? Result.Success() : Result.Failure(Error.CannotSaveChanges);
    }
}
