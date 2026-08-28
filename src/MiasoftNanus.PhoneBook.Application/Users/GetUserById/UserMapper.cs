using MiasoftNanus.PhoneBook.Domain.Users.Entities;

namespace MiasoftNanus.PhoneBook.Application.Users.GetUserById;

/// <summary>
/// Provides a mapping utility for converting a User entity into a GetUserByIdResult record.
/// This class is used as part of the application layer for transforming domain user entities
/// into a format suited for application queries.
/// </summary>
public static class UserMapper
{
    /// <summary>
    /// Maps a <see cref="User"/> entity to a <see cref="GetUserByIdResult"/> record.
    /// </summary>
    /// <param name="user">The user entity to be mapped.</param>
    /// <returns>A <see cref="GetUserByIdResult"/> instance containing the user's details.</returns>
    public static GetUserByIdResult MapToGetUserByIdResult(this User user)
    {
        return new GetUserByIdResult(
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
    }
}