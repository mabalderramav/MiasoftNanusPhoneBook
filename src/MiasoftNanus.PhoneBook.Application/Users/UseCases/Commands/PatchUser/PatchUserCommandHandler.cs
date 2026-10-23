using MiasoftNanus.PhoneBook.Application.Abstractions.CQRS;
using MiasoftNanus.PhoneBook.Application.Abstractions.Time;
using MiasoftNanus.PhoneBook.Domain.Abstractions;
using MiasoftNanus.PhoneBook.Domain.Profiles.Errors;
using MiasoftNanus.PhoneBook.Domain.Profiles.Repositories;
using MiasoftNanus.PhoneBook.Domain.Users.Entities;
using MiasoftNanus.PhoneBook.Domain.Users.Errors;
using MiasoftNanus.PhoneBook.Domain.Users.Repositories;

namespace MiasoftNanus.PhoneBook.Application.Users.UseCases.Commands.PatchUser;

/// <summary>
/// Applies and persists a partial update of the details of an existing user. The stored password hash is kept as is.
/// </summary>
public sealed class PatchUserCommandHandler(
    IUserRepository userRepository,
    IProfileRepository profileRepository,
    IUnitOfWork unitOfWork,
    IDateTimeProvider dateTimeProvider
) : ICommandHandler<PatchUserCommand, Result<User>>
{
    public async Task<Result<User>> HandleAsync(
        PatchUserCommand command,
        CancellationToken cancellationToken = default)
    {
        var user = await userRepository.GetByIdAsync(command.Id, cancellationToken);
        if (user is null)
            return Result.Failure<User>(UserErrors.UserNotFound);

        // Nothing to change: no save (it would affect 0 rows) and no DateOfLastChange bump.
        if (!command.Data.HasChanges)
            return Result.Success(user);

        var patchResult = user.Patch(command.Data, dateTimeProvider.CurrentTime);
        if (patchResult.IsFailure)
            return Result.Failure<User>(patchResult.Error);

        if (command.Data.ProfileId is not null)
        {
            var profile = await profileRepository.GetByIdAsync(command.Data.ProfileId.Value, cancellationToken);
            if (profile is null)
                return Result.Failure<User>(ProfileErrors.ProfileNotFound);
        }

        userRepository.Update(user);
        var result = await unitOfWork.SaveChangesAsync(cancellationToken);

        return result > 0
            ? Result.Success(user)
            : Result.Failure<User>(Error.CannotSaveChanges);
    }
}
