using MiasoftNanus.PhoneBook.Application.Abstractions.CQRS;
using MiasoftNanus.PhoneBook.Application.Abstractions.Time;
using MiasoftNanus.PhoneBook.Application.Users.Mappers;
using MiasoftNanus.PhoneBook.Domain.Abstractions;
using MiasoftNanus.PhoneBook.Domain.Profiles.Errors;
using MiasoftNanus.PhoneBook.Domain.Profiles.Repositories;
using MiasoftNanus.PhoneBook.Domain.Users.Entities;
using MiasoftNanus.PhoneBook.Domain.Users.Errors;
using MiasoftNanus.PhoneBook.Domain.Users.Repositories;
using MiasoftNanus.PhoneBook.Domain.Users.Services;

namespace MiasoftNanus.PhoneBook.Application.Users.UseCases.Commands.UpdateUser;

/// <summary>
/// Updates and persists an existing user.
/// </summary>
public sealed class UpdateUserCommandHandler(
    IUserRepository userRepository,
    IProfileRepository profileRepository,
    IUnitOfWork unitOfWork,
    IDateTimeProvider dateTimeProvider,
    IPasswordHasher passwordHasher
) : ICommandHandler<UpdateUserCommand, Result<User>>
{
    public async Task<Result<User>> HandleAsync(
        UpdateUserCommand command,
        CancellationToken cancellationToken = default)
    {
        var user = await userRepository.GetByIdAsync(command.Id, cancellationToken);
        if (user is null)
            return Result.Failure<User>(UserErrors.UserNotFound);

        var updateResult = user.Update(command.ToUserCreationData(dateTimeProvider.CurrentTime));
        if (updateResult.IsFailure)
            return Result.Failure<User>(updateResult.Error);

        var profile = await profileRepository.GetByIdAsync(command.ProfileId, cancellationToken);
        if (profile is null)
            return Result.Failure<User>(ProfileErrors.ProfileNotFound);

        // Hash after the inexpensive checks: PBKDF2 is intentionally slow.
        user.HashPassword(passwordHasher);

        userRepository.Update(user);
        var result = await unitOfWork.SaveChangesAsync(cancellationToken);

        return result > 0
            ? Result.Success(user)
            : Result.Failure<User>(Error.CannotSaveChanges);
    }
}
