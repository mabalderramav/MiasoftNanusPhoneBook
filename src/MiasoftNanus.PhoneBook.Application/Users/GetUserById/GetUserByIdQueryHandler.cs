using MiasoftNanus.PhoneBook.Application.Abstractions.Messaging;
using MiasoftNanus.PhoneBook.Domain.Abstractions;
using MiasoftNanus.PhoneBook.Domain.Users.Errors;
using MiasoftNanus.PhoneBook.Domain.Users.Repositories;

namespace MiasoftNanus.PhoneBook.Application.Users.GetUserById;

/// <summary>
/// Handles the query to retrieve a user by their unique identifier.
/// </summary>
/// <remarks>
/// This handler processes a query of type <see cref="GetUserByIdQuery"/> and retrieves the corresponding user details
/// from the data source using the <see cref="IUserRepository"/>.
/// It returns the result as an instance of <see cref="GetUserByIdResult"/> encapsulated in a <see cref="Result{T}"/>
/// object.
/// </remarks>
/// <exception cref="UserErrors.UserNotFound">
/// Thrown when the user with the provided identifier does not exist in the system.
/// </exception>
internal sealed class GetUserByIdQueryHandler(IUserRepository userRepository)
    : IQueryHandler<GetUserByIdQuery, GetUserByIdResult>
{
    public async Task<Result<GetUserByIdResult>> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByIdAsync(request.Id, cancellationToken);
        if (user is null)
        {
            return Result.Failure<GetUserByIdResult>(UserErrors.UserNotFound);
        }

        var getUserByResult = user.MapToGetUserByIdResult();

        return Result.Success(getUserByResult);
    }
}