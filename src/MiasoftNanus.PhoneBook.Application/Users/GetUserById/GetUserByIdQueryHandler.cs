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
/// It returns the result as an instance of <see cref="GetUserByResult"/> encapsulated in a <see cref="Result{T}"/>
/// object.
/// </remarks>
/// <exception cref="UserErrors.UserNotFound">
/// Thrown when the user with the provided identifier does not exist in the system.
/// </exception>
internal sealed class GetUserByIdQueryHandler(IUserRepository userRepository)
    : IQueryHandler<GetUserByIdQuery, GetUserByResult>
{
    public async Task<Result<GetUserByResult>> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByIdAsync(request.Id, cancellationToken);
        if (user is null)
        {
            return Result.Failure<GetUserByResult>(UserErrors.UserNotFound);
        }

        var getUserByResult = new GetUserByResult(
            user.Id,
            user.FirstName!,
            user.LastName!,
            user.UserName!.Value,
            user.Birthdate,
            user.Email!.Value,
            user.Address!.Country,
            user.Address.State,
            user.Address.Province,
            user.Address.District,
            user.Address.Street,
            user.States,
            user.Profile!.ProfileName!,
            user.DateOfLastChange
        );

        return Result.Success(getUserByResult);
    }
}