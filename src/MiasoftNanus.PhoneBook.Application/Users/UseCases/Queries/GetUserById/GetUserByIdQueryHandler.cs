using MiasoftNanus.PhoneBook.Application.Abstractions.CQRS;
using MiasoftNanus.PhoneBook.Domain.Abstractions;
using MiasoftNanus.PhoneBook.Domain.Users.Entities;
using MiasoftNanus.PhoneBook.Domain.Users.Errors;
using MiasoftNanus.PhoneBook.Domain.Users.Repositories;

namespace MiasoftNanus.PhoneBook.Application.Users.UseCases.Queries.GetUserById;

/// <summary>
/// Retrieves a user by identifier from the repository.
/// </summary>
public class GetUserByIdQueryHandler(IUserRepository userRepository) :
    IQueryHandler<GetUserByIdQuery, Result<User>>
{
    public async Task<Result<User>> HandleAsync(
        GetUserByIdQuery query,
        CancellationToken cancellationToken = default)
    {
        var user = await userRepository.GetByIdAsync(query.Id, cancellationToken);
        return user is not null
            ? Result.Success(user)
            : Result.Failure<User>(UserErrors.UserNotFound);
    }
}
