using MiasoftNanus.PhoneBook.Application.Abstractions.CQRS;
using MiasoftNanus.PhoneBook.Domain.Abstractions;
using MiasoftNanus.PhoneBook.Domain.WorkAreas.Entities;
using MiasoftNanus.PhoneBook.Domain.WorkAreas.Errors;
using MiasoftNanus.PhoneBook.Domain.WorkAreas.Repositories;

namespace MiasoftNanus.PhoneBook.Application.WorkAreas.UseCases.Commands.UpdateWorkArea;

/// <summary>
/// Updates and persists an existing work area.
/// </summary>
public sealed class UpdateWorkAreaCommandHandler(
    IWorkAreaRepository workAreaRepository,
    IUnitOfWork unitOfWork
) : ICommandHandler<UpdateWorkAreaCommand, Result<WorkArea>>
{
    public async Task<Result<WorkArea>> HandleAsync(
        UpdateWorkAreaCommand command,
        CancellationToken cancellationToken = default)
    {
        var workArea = await workAreaRepository.GetByIdAsync(command.Id, cancellationToken);
        if (workArea is null)
            return Result.Failure<WorkArea>(WorkAreaErrors.WorkAreaNotFound);

        var updateResult = workArea.Update(command.Code, command.Name, command.Description);
        if (updateResult.IsFailure)
            return Result.Failure<WorkArea>(updateResult.Error);

        workAreaRepository.Update(workArea);
        var result = await unitOfWork.SaveChangesAsync(cancellationToken);

        return result > 0
            ? Result.Success(workArea)
            : Result.Failure<WorkArea>(Error.CannotSaveChanges);
    }
}
