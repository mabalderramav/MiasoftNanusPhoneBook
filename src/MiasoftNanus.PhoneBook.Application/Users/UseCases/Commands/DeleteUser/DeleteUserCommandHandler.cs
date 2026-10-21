using MiasoftNanus.PhoneBook.Application.Abstractions.CQRS;
using MiasoftNanus.PhoneBook.Domain.Abstractions;
using MiasoftNanus.PhoneBook.Domain.Users.Errors;
using MiasoftNanus.PhoneBook.Domain.Users.Repositories;

namespace MiasoftNanus.PhoneBook.Application.Users.UseCases.Commands.DeleteUser;

/// <summary>
/// Deletes and persists removal of an existing user.
/// </summary>
public sealed class DeleteUserCommandHandler(
    IUserRepository userRepository,
    IUnitOfWork unitOfWork
) : ICommandHandler<DeleteUserCommand, Result>
{
    public async Task<Result> HandleAsync(
        DeleteUserCommand command,
        CancellationToken cancellationToken = default)
    {
        var user = await userRepository.GetByIdAsync(command.Id, cancellationToken);
        if (user is null)
            return Result.Failure(UserErrors.UserNotFound);

        userRepository.Remove(user);
        var result = await unitOfWork.SaveChangesAsync(cancellationToken);

        return result > 0 ? Result.Success() : Result.Failure(Error.CannotSaveChanges);
    }
}
