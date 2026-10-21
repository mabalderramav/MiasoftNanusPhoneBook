using MiasoftNanus.PhoneBook.Application.Abstractions.CQRS;
using MiasoftNanus.PhoneBook.Domain.Abstractions;
using MiasoftNanus.PhoneBook.Domain.WorkAreas.Errors;
using MiasoftNanus.PhoneBook.Domain.WorkAreas.Repositories;
using MiasoftNanus.PhoneBook.Domain.Workstations.Entities;
using MiasoftNanus.PhoneBook.Domain.Workstations.Errors;
using MiasoftNanus.PhoneBook.Domain.Workstations.Repositories;

namespace MiasoftNanus.PhoneBook.Application.Workstations.UseCases.Commands.UpdateWorkstation;

/// <summary>
/// Updates and persists an existing workstation.
/// </summary>
public sealed class UpdateWorkstationCommandHandler(
    IWorkstationRepository workstationRepository,
    IWorkAreaRepository workAreaRepository,
    IUnitOfWork unitOfWork
) : ICommandHandler<UpdateWorkstationCommand, Result<Workstation>>
{
    public async Task<Result<Workstation>> HandleAsync(
        UpdateWorkstationCommand command,
        CancellationToken cancellationToken = default)
    {
        var workstation = await workstationRepository.GetByIdAsync(command.Id, cancellationToken);
        if (workstation is null)
            return Result.Failure<Workstation>(WorkstationErrors.WorkstationNotFound);

        var updateResult = workstation.Update(
            command.Code,
            command.Name,
            command.Description,
            command.WorkAreaId);
        if (updateResult.IsFailure)
            return Result.Failure<Workstation>(updateResult.Error);

        var workArea = await workAreaRepository.GetByIdAsync(command.WorkAreaId, cancellationToken);
        if (workArea is null)
            return Result.Failure<Workstation>(WorkAreaErrors.WorkAreaNotFound);

        workstationRepository.Update(workstation);
        var result = await unitOfWork.SaveChangesAsync(cancellationToken);

        return result > 0
            ? Result.Success(workstation)
            : Result.Failure<Workstation>(Error.CannotSaveChanges);
    }
}
