using MiasoftNanus.PhoneBook.Application.Abstractions.CQRS;
using MiasoftNanus.PhoneBook.Domain.Abstractions;
using MiasoftNanus.PhoneBook.Domain.Profiles.Entities;
using MiasoftNanus.PhoneBook.Domain.Profiles.Repositories;

namespace MiasoftNanus.PhoneBook.Application.Profiles.UseCases.Commands.CreateProfile;

/// <summary>
/// Handles the execution of the command to create a new profile within the system.
/// </summary>
/// <remarks>
/// This class is responsible for validating the <see cref="CreateProfileCommand"/>, creating a new profile
/// entity, and persisting it using the repository pattern and unit of work. It ensures that all changes
/// are committed atomically.
/// </remarks>
/// <example>
/// This class is typically invoked by passing a <see cref="CreateProfileCommand"/> containing the profile
/// details, such as name and description. Upon successful execution, it returns the created profile wrapped
/// in a <see cref="Result{TValue}"/> object, or an error if the operation failed.
/// </example>
/// <typeparam>The result of the operation, indicating success or failure and containing the created entity
/// upon success.
/// <name>Result{Profile}</name>
/// </typeparam>
public sealed class CreateProfileCommandHandler(
    IProfileRepository profileRepository,
    IUnitOfWork unitOfWork
) : ICommandHandler<CreateProfileCommand, Result<Profile>>
{
    public async Task<Result<Profile>> HandleAsync(
        CreateProfileCommand command,
        CancellationToken cancellationToken = default)
    {
        // Create a new Profile entity and validate its properties using the static Create method
        var profileResult = Profile.Create(command.ProfileName, command.Description);
        if (profileResult.IsFailure)
            return Result.Failure<Profile>(profileResult.Error);

        // Add the new Profile entity to the repository
        profileRepository.Add(profileResult.Value);

        // Save changes to the database and check if the operation was successful
        var result = await unitOfWork.SaveChangesAsync(cancellationToken);

        return result > 0 ? Result.Success(profileResult.Value) : Result.Failure<Profile>(Error.CannotSaveChanges);
    }
}