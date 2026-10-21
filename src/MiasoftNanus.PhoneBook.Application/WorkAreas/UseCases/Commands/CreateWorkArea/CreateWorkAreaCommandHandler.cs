using MiasoftNanus.PhoneBook.Application.Abstractions.CQRS;
using MiasoftNanus.PhoneBook.Domain.Abstractions;
using MiasoftNanus.PhoneBook.Domain.WorkAreas.Entities;
using MiasoftNanus.PhoneBook.Domain.WorkAreas.Repositories;

namespace MiasoftNanus.PhoneBook.Application.WorkAreas.UseCases.Commands.CreateWorkArea;

/// <summary>
/// Creates and persists a work area.
/// </summary>
public sealed class CreateWorkAreaCommandHandler(
    IWorkAreaRepository workAreaRepository,
    IUnitOfWork unitOfWork
) : ICommandHandler<CreateWorkAreaCommand, Result<WorkArea>>
{
    public async Task<Result<WorkArea>> HandleAsync(
        CreateWorkAreaCommand command,
        CancellationToken cancellationToken = default)
    {
        var workAreaResult = WorkArea.Create(command.Code, command.Name, command.Description);
        if (workAreaResult.IsFailure)
            return Result.Failure<WorkArea>(workAreaResult.Error);

        workAreaRepository.Add(workAreaResult.Value);
        var result = await unitOfWork.SaveChangesAsync(cancellationToken);

        return result > 0
            ? Result.Success(workAreaResult.Value)
            : Result.Failure<WorkArea>(Error.CannotSaveChanges);
    }
}
