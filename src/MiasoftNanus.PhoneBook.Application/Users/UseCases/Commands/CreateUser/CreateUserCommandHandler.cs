using MiasoftNanus.PhoneBook.Application.Abstractions.CQRS;
using MiasoftNanus.PhoneBook.Application.Abstractions.Time;
using MiasoftNanus.PhoneBook.Application.Users.Mappers;
using MiasoftNanus.PhoneBook.Domain.Abstractions;
using MiasoftNanus.PhoneBook.Domain.Profiles.Errors;
using MiasoftNanus.PhoneBook.Domain.Profiles.Repositories;
using MiasoftNanus.PhoneBook.Domain.Users.Entities;
using MiasoftNanus.PhoneBook.Domain.Users.Repositories;
using MiasoftNanus.PhoneBook.Domain.Users.Services;

namespace MiasoftNanus.PhoneBook.Application.Users.UseCases.Commands.CreateUser;

/// <summary>
/// Creates and persists a new user.
/// </summary>
public sealed class CreateUserCommandHandler(
    IUserRepository userRepository,
    IProfileRepository profileRepository,
    IUnitOfWork unitOfWork,
    IDateTimeProvider dateTimeProvider,
    IPasswordHasher passwordHasher
) : ICommandHandler<CreateUserCommand, Result<User>>
{
    public async Task<Result<User>> HandleAsync(
        CreateUserCommand command,
        CancellationToken cancellationToken = default)
    {
        var userResult = User.Create(command.ToUserCreationData(dateTimeProvider.CurrentTime));
        if (userResult.IsFailure)
            return Result.Failure<User>(userResult.Error);

        var profile = await profileRepository.GetByIdAsync(command.ProfileId, cancellationToken);
        if (profile is null)
            return Result.Failure<User>(ProfileErrors.ProfileNotFound);

        // Hash after the cheap checks: PBKDF2 is intentionally slow.
        userResult.Value.HashPassword(passwordHasher);

        userRepository.Add(userResult.Value);
        var result = await unitOfWork.SaveChangesAsync(cancellationToken);

        return result > 0
            ? Result.Success(userResult.Value)
            : Result.Failure<User>(Error.CannotSaveChanges);
    }
}
