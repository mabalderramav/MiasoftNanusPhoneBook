using MiasoftNanus.PhoneBook.Application.Abstractions.CQRS;
using MiasoftNanus.PhoneBook.Domain.Abstractions;
using MiasoftNanus.PhoneBook.Domain.WorkAreas.Errors;
using MiasoftNanus.PhoneBook.Domain.WorkAreas.Repositories;
using MiasoftNanus.PhoneBook.Domain.Workstations.Entities;
using MiasoftNanus.PhoneBook.Domain.Workstations.Repositories;

namespace MiasoftNanus.PhoneBook.Application.Workstations.UseCases.Commands.CreateWorkstation;

/// <summary>
/// Creates and persists a workstation.
/// </summary>
public sealed class CreateWorkstationCommandHandler(
    IWorkstationRepository workstationRepository,
    IWorkAreaRepository workAreaRepository,
    IUnitOfWork unitOfWork
) : ICommandHandler<CreateWorkstationCommand, Result<Workstation>>
{
    public async Task<Result<Workstation>> HandleAsync(
        CreateWorkstationCommand command,
        CancellationToken cancellationToken = default)
    {
        var workstationResult = Workstation.Create(
            command.Code,
            command.Name,
            command.Description,
            command.WorkAreaId);
        if (workstationResult.IsFailure)
            return Result.Failure<Workstation>(workstationResult.Error);

        var workArea = await workAreaRepository.GetByIdAsync(command.WorkAreaId, cancellationToken);
        if (workArea is null)
            return Result.Failure<Workstation>(WorkAreaErrors.WorkAreaNotFound);

        workstationRepository.Add(workstationResult.Value);
        var result = await unitOfWork.SaveChangesAsync(cancellationToken);

        return result > 0
            ? Result.Success(workstationResult.Value)
            : Result.Failure<Workstation>(Error.CannotSaveChanges);
    }
}
