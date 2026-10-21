using MiasoftNanus.PhoneBook.Application.Abstractions.CQRS;
using MiasoftNanus.PhoneBook.Domain.Abstractions;
using MiasoftNanus.PhoneBook.Domain.Workstations.Errors;
using MiasoftNanus.PhoneBook.Domain.Workstations.Repositories;

namespace MiasoftNanus.PhoneBook.Application.Workstations.UseCases.Commands.DeleteWorkstation;

/// <summary>
/// Deletes and persists removal of an existing workstation.
/// </summary>
public sealed class DeleteWorkstationCommandHandler(
    IWorkstationRepository workstationRepository,
    IUnitOfWork unitOfWork
) : ICommandHandler<DeleteWorkstationCommand, Result>
{
    public async Task<Result> HandleAsync(
        DeleteWorkstationCommand command,
        CancellationToken cancellationToken = default)
    {
        var workstation = await workstationRepository.GetByIdAsync(command.Id, cancellationToken);
        if (workstation is null)
            return Result.Failure(WorkstationErrors.WorkstationNotFound);

        workstationRepository.Remove(workstation);
        var result = await unitOfWork.SaveChangesAsync(cancellationToken);

        return result > 0 ? Result.Success() : Result.Failure(Error.CannotSaveChanges);
    }
}
