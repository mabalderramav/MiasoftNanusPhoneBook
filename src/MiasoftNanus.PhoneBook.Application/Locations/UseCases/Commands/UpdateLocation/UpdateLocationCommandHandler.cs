using MiasoftNanus.PhoneBook.Application.Abstractions.CQRS;
using MiasoftNanus.PhoneBook.Domain.Abstractions;
using MiasoftNanus.PhoneBook.Domain.Locations.Entities;
using MiasoftNanus.PhoneBook.Domain.Locations.Errors;
using MiasoftNanus.PhoneBook.Domain.Locations.Repositories;

namespace MiasoftNanus.PhoneBook.Application.Locations.UseCases.Commands.UpdateLocation;

/// <summary>
/// Handles the execution of the command to update an existing location within the system.
/// </summary>
/// <remarks>
/// This class is responsible for retrieving the location targeted by the <see cref="UpdateLocationCommand"/>,
/// applying and validating the changes, and persisting them using the repository pattern and unit of work.
/// It ensures that all changes are committed atomically.
/// </remarks>
/// <example>
/// This class is typically invoked by passing a <see cref="UpdateLocationCommand"/> containing the location
/// identifier and the new details, such as code and description. Upon successful execution, it returns the
/// updated location wrapped in a <see cref="Result{TValue}"/> object, or an error if the operation failed.
/// </example>
/// <typeparam>The result of the operation, indicating success or failure and containing the updated entity
/// upon success.
/// <name>Result{Location}</name>
/// </typeparam>
public sealed class UpdateLocationCommandHandler(
    ILocationRepository locationRepository,
    IUnitOfWork unitOfWork
) : ICommandHandler<UpdateLocationCommand, Result<Location>>
{
    public async Task<Result<Location>> HandleAsync(
        UpdateLocationCommand command,
        CancellationToken cancellationToken = default)
    {
        // Retrieve the existing Location entity
        var location = await locationRepository.GetByIdAsync(command.Id, cancellationToken);
        if (location is null)
            return Result.Failure<Location>(LocationErrors.LocationNotFound);

        // Apply the changes and validate them using the Update method
        var updateResult = location.Update(command.Code, command.Description);
        if (updateResult.IsFailure)
            return Result.Failure<Location>(updateResult.Error);

        // Mark the Location entity as updated in the repository
        locationRepository.Update(location);

        // Save changes to the database and check if the operation was successful
        var result = await unitOfWork.SaveChangesAsync(cancellationToken);

        return result > 0 ? Result.Success(location) : Result.Failure<Location>(Error.CannotSaveChanges);
    }
}
