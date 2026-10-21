using MiasoftNanus.PhoneBook.Application.Abstractions.CQRS;
using MiasoftNanus.PhoneBook.Domain.Abstractions;
using MiasoftNanus.PhoneBook.Domain.Users.Entities;
using MiasoftNanus.PhoneBook.Domain.Users.Errors;
using MiasoftNanus.PhoneBook.Domain.Users.Repositories;

namespace MiasoftNanus.PhoneBook.Application.Users.UseCases.Queries.GetUserAll;

/// <summary>
/// Retrieves all users from the repository.
/// </summary>
public class GetUserAllQueryHandler(IUserRepository userRepository) :
    IQueryHandler<GetUserAllQuery, Result<ICollection<User>>>
{
    public async Task<Result<ICollection<User>>> HandleAsync(
        GetUserAllQuery query,
        CancellationToken cancellationToken = default)
    {
        var users = await userRepository.GetAllAsync(cancellationToken);
        return users.Count > 0
            ? Result.Success(users)
            : Result.Failure<ICollection<User>>(UserErrors.UsersNotFound);
    }
}
