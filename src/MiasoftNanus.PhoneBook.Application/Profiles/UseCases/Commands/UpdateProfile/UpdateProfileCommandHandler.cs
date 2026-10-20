using MiasoftNanus.PhoneBook.Application.Abstractions.CQRS;
using MiasoftNanus.PhoneBook.Domain.Abstractions;
using MiasoftNanus.PhoneBook.Domain.Profiles.Entities;
using MiasoftNanus.PhoneBook.Domain.Profiles.Errors;
using MiasoftNanus.PhoneBook.Domain.Profiles.Repositories;

namespace MiasoftNanus.PhoneBook.Application.Profiles.UseCases.Commands.UpdateProfile;

/// <summary>
/// Handles the execution of the command to update an existing profile within the system.
/// </summary>
/// <remarks>
/// This class is responsible for retrieving the profile targeted by the <see cref="UpdateProfileCommand"/>,
/// applying and validating the changes, and persisting them using the repository pattern and unit of work.
/// It ensures that all changes are committed atomically.
/// </remarks>
/// <example>
/// This class is typically invoked by passing a <see cref="UpdateProfileCommand"/> containing the profile
/// identifier and the new details, such as name and description. Upon successful execution, it returns the
/// updated profile wrapped in a <see cref="Result{TValue}"/> object, or an error if the operation failed.
/// </example>
/// <typeparam>The result of the operation, indicating success or failure and containing the updated entity
/// upon success.
/// <name>Result{Profile}</name>
/// </typeparam>
public sealed class UpdateProfileCommandHandler(
    IProfileRepository profileRepository,
    IUnitOfWork unitOfWork
) : ICommandHandler<UpdateProfileCommand, Result<Profile>>
{
    public async Task<Result<Profile>> HandleAsync(
        UpdateProfileCommand command,
        CancellationToken cancellationToken = default)
    {
        // Retrieve the existing Profile entity
        var profile = await profileRepository.GetByIdAsync(command.Id, cancellationToken);
        if (profile is null)
            return Result.Failure<Profile>(ProfileErrors.ProfileNotFound);

        // Apply the changes and validate them using the Update method
        var updateResult = profile.Update(command.ProfileName, command.Description);
        if (updateResult.IsFailure)
            return Result.Failure<Profile>(updateResult.Error);

        // Mark the Profile entity as updated in the repository
        profileRepository.Update(profile);

        // Save changes to the database and check if the operation was successful
        var result = await unitOfWork.SaveChangesAsync(cancellationToken);

        return result > 0 ? Result.Success(profile) : Result.Failure<Profile>(Error.CannotSaveChanges);
    }
}
