using MiasoftNanus.PhoneBook.Application.Abstractions.CQRS;
using MiasoftNanus.PhoneBook.Domain.Abstractions;
using MiasoftNanus.PhoneBook.Domain.Locations.Errors;
using MiasoftNanus.PhoneBook.Domain.Locations.Repositories;

namespace MiasoftNanus.PhoneBook.Application.Locations.UseCases.Commands.DeleteLocation;

/// <summary>
/// Handles the execution of the command to delete an existing location from the system.
/// </summary>
/// <remarks>
/// This class is responsible for retrieving the location targeted by the <see cref="DeleteLocationCommand"/>,
/// removing it, and persisting the change using the repository pattern and unit of work. It ensures that
/// all changes are committed atomically.
/// </remarks>
/// <example>
/// This class is typically invoked by passing a <see cref="DeleteLocationCommand"/> containing the location
/// identifier. Upon successful execution, it returns a successful <see cref="Result"/>, or an error if the
/// location was not found or the operation failed.
/// </example>
/// <typeparam>The result of the operation, indicating success or failure.
/// <name>Result</name>
/// </typeparam>
public sealed class DeleteLocationCommandHandler(
    ILocationRepository locationRepository,
    IUnitOfWork unitOfWork
) : ICommandHandler<DeleteLocationCommand, Result>
{
    public async Task<Result> HandleAsync(
        DeleteLocationCommand command,
        CancellationToken cancellationToken = default)
    {
        // Retrieve the existing Location entity
        var location = await locationRepository.GetByIdAsync(command.Id, cancellationToken);
        if (location is null)
            return Result.Failure(LocationErrors.LocationNotFound);

        // Remove the Location entity from the repository
        locationRepository.Remove(location);

        // Save changes to the database and check if the operation was successful
        var result = await unitOfWork.SaveChangesAsync(cancellationToken);

        return result > 0 ? Result.Success() : Result.Failure(Error.CannotSaveChanges);
    }
}
