using MiasoftNanus.PhoneBook.Application.Abstractions.CQRS;
using MiasoftNanus.PhoneBook.Application.Abstractions.Time;
using MiasoftNanus.PhoneBook.Domain.Abstractions;
using MiasoftNanus.PhoneBook.Domain.Users.Errors;
using MiasoftNanus.PhoneBook.Domain.Users.Repositories;
using MiasoftNanus.PhoneBook.Domain.Users.Services;

namespace MiasoftNanus.PhoneBook.Application.Users.UseCases.Commands.ChangeUserPassword;

/// <summary>
/// Validates, hashes and persists a new password for an existing user.
/// </summary>
public sealed class ChangeUserPasswordCommandHandler(
    IUserRepository userRepository,
    IUnitOfWork unitOfWork,
    IDateTimeProvider dateTimeProvider,
    IPasswordHasher passwordHasher
) : ICommandHandler<ChangeUserPasswordCommand, Result>
{
    public async Task<Result> HandleAsync(
        ChangeUserPasswordCommand command,
        CancellationToken cancellationToken = default)
    {
        var user = await userRepository.GetByIdAsync(command.Id, cancellationToken);
        if (user is null)
            return Result.Failure(UserErrors.UserNotFound);

        var changeResult = user.ChangePassword(command.Password, passwordHasher, dateTimeProvider.CurrentTime);
        if (changeResult.IsFailure)
            return changeResult;

        userRepository.Update(user);
        var result = await unitOfWork.SaveChangesAsync(cancellationToken);

        return result > 0 ? Result.Success() : Result.Failure(Error.CannotSaveChanges);
    }
}
