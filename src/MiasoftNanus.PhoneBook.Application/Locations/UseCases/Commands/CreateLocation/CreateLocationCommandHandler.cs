using MiasoftNanus.PhoneBook.Application.Abstractions.CQRS;
using MiasoftNanus.PhoneBook.Domain.Abstractions;
using MiasoftNanus.PhoneBook.Domain.Locations.Entities;
using MiasoftNanus.PhoneBook.Domain.Locations.Repositories;

namespace MiasoftNanus.PhoneBook.Application.Locations.UseCases.Commands.CreateLocation;

/// <summary>
/// Handles the execution of the command to create a new location within the system.
/// </summary>
/// <remarks>
/// This class is responsible for validating the <see cref="CreateLocationCommand"/>, creating a new location
/// entity, and persisting it using the repository pattern and unit of work. It ensures that all changes
/// are committed atomically.
/// </remarks>
/// <example>
/// This class is typically invoked by passing a <see cref="CreateLocationCommand"/> containing the location
/// details, such as code and description. Upon successful execution, it returns the created location wrapped
/// in a <see cref="Result{TValue}"/> object, or an error if the operation failed.
/// </example>
/// <typeparam>The result of the operation, indicating success or failure and containing the created entity
/// upon success.
/// <name>Result{Location}</name>
/// </typeparam>
public sealed class CreateLocationCommandHandler(
    ILocationRepository locationRepository,
    IUnitOfWork unitOfWork
) : ICommandHandler<CreateLocationCommand, Result<Location>>
{
    public async Task<Result<Location>> HandleAsync(
        CreateLocationCommand command,
        CancellationToken cancellationToken = default)
    {
        // Create a new Location entity and validate its properties using the static Create method
        var locationResult = Location.Create(command.Code, command.Description);
        if (locationResult.IsFailure)
            return Result.Failure<Location>(locationResult.Error);

        // Add the new Location entity to the repository
        locationRepository.Add(locationResult.Value);

        // Save changes to the database and check if the operation was successful
        var result = await unitOfWork.SaveChangesAsync(cancellationToken);

        return result > 0 ? Result.Success(locationResult.Value) : Result.Failure<Location>(Error.CannotSaveChanges);
    }
}
