using MiasoftNanus.PhoneBook.Application.Abstractions.CQRS;
using MiasoftNanus.PhoneBook.Domain.Abstractions;
using MiasoftNanus.PhoneBook.Domain.Profiles.Errors;
using MiasoftNanus.PhoneBook.Domain.Profiles.Repositories;

namespace MiasoftNanus.PhoneBook.Application.Profiles.UseCases.Commands.DeleteProfile;

/// <summary>
/// Handles the execution of the command to delete an existing profile from the system.
/// </summary>
/// <remarks>
/// This class is responsible for retrieving the profile targeted by the <see cref="DeleteProfileCommand"/>,
/// removing it, and persisting the change using the repository pattern and unit of work. It ensures that
/// all changes are committed atomically.
/// </remarks>
/// <example>
/// This class is typically invoked by passing a <see cref="DeleteProfileCommand"/> containing the profile
/// identifier. Upon successful execution, it returns a successful <see cref="Result"/>, or an error if the
/// profile was not found or the operation failed.
/// </example>
/// <typeparam>The result of the operation, indicating success or failure.
/// <name>Result</name>
/// </typeparam>
public sealed class DeleteProfileCommandHandler(
    IProfileRepository profileRepository,
    IUnitOfWork unitOfWork
) : ICommandHandler<DeleteProfileCommand, Result>
{
    public async Task<Result> HandleAsync(
        DeleteProfileCommand command,
        CancellationToken cancellationToken = default)
    {
        // Retrieve the existing Profile entity
        var profile = await profileRepository.GetByIdAsync(command.Id, cancellationToken);
        if (profile is null)
            return Result.Failure(ProfileErrors.ProfileNotFound);

        // Remove the Profile entity from the repository
        profileRepository.Remove(profile);

        // Save changes to the database and check if the operation was successful
        var result = await unitOfWork.SaveChangesAsync(cancellationToken);

        return result > 0 ? Result.Success() : Result.Failure(Error.CannotSaveChanges);
    }
}
