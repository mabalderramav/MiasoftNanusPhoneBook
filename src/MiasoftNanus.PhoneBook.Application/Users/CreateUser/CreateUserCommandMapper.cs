using MiasoftNanus.PhoneBook.Application.Abstractions.Time;
using MiasoftNanus.PhoneBook.Domain.Profiles.Entities;
using MiasoftNanus.PhoneBook.Domain.Users.Entities;
using MiasoftNanus.PhoneBook.Domain.Users.ObjectValues;

namespace MiasoftNanus.PhoneBook.Application.Users.CreateUser;

/// <summary>
/// Provides mapping functionality to convert a <see cref="CreateUserCommand"/>
/// and associated data into a <see cref="UserCreationData"/> object for creating
/// new user entities.
/// </summary>
public static class CreateUserCommandMapper
{
    /// <summary>
    /// Maps a <see cref="CreateUserCommand"/> to a <see cref="UserCreationData"/> instance containing
    /// user information, including address, email, password, and more, to facilitate user creation.
    /// </summary>
    /// <param name="request">The command containing the user details to be mapped.</param>
    /// <param name="password">The user's password wrapped in a <see cref="Password"/> object.</param>
    /// <param name="email">The user's email wrapped in an <see cref="Email"/> object.</param>
    /// <param name="profile">The profile data of the user.</param>
    /// <param name="dateTimeProvider">An implementation of <see cref="IDateTimeProvider"/> to obtain the current time.</param>
    /// <returns>An instance of <see cref="UserCreationData"/> containing the mapped user information.</returns>
    public static UserCreationData MapToUserCreationData(this CreateUserCommand request,
        Password password,
        Email email,
        Profile profile,
        IDateTimeProvider dateTimeProvider)
    {
        var addressResult = new Address(
            request.Country,
            request.State,
            request.Province,
            request.District,
            request.Street
        );
        
        return new UserCreationData()
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Password = password,
            Birthdate = request.Birthdate.ToUniversalTime(),
            Email = email,
            Address = addressResult,
            States = request.States,
            DateOfLastChange = dateTimeProvider.CurrentTime,
            ProfileId = profile.Id
        };
    }
}