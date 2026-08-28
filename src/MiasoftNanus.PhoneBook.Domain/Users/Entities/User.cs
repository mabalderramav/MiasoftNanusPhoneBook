using MiasoftNanus.PhoneBook.Domain.Abstractions;
using MiasoftNanus.PhoneBook.Domain.Profiles.Entities;
using MiasoftNanus.PhoneBook.Domain.Shared;
using MiasoftNanus.PhoneBook.Domain.Users.Events;
using MiasoftNanus.PhoneBook.Domain.Users.ObjectValues;
using MiasoftNanus.PhoneBook.Domain.Users.Services;

namespace MiasoftNanus.PhoneBook.Domain.Users.Entities;

/// <summary>
/// Represents a user entity in the phonebook domain.
/// </summary>
/// <remarks>
/// The User class provides properties and methods to manage the information associated with a user,
/// including personal identification data, contact details, and domain-specific information required
/// for operation within the phonebook system.
/// </remarks>
/// <example>
/// This class serves as the core model for managing user-specific data and behaviors within the domain.
/// It includes various properties such as FirstName, LastName, and Email, along with domain-specific
/// methods for user creation and event handling.
/// </example>
public class User : Entity
{
    private User(
        Guid id,
        UserCreationData data,
        Username userName
    ) : base(id)
    {
        FirstName = data.FirstName;
        LastName = data.LastName;
        Password = data.Password;
        UserName = userName;
        Birthdate = data.Birthdate;
        Email = data.Email;
        Address = data.Address;
        States = data.States;
        DateOfLastChange = data.DateOfLastChange;
        ProfileId = data.ProfileId;
    }
    
    protected User()
    {
    }

    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public Password? Password { get; set; }
    public Username? UserName { get; set; }
    public DateTime Birthdate { get; set; }
    public Email? Email { get; set; }
    public Address? Address { get; set; }
    public States? States { get; set; }
    public DateTime DateOfLastChange { get; set; }
    public Guid ProfileId { get; set; }
    public Profile? Profile { get; set; }

    /// <summary>
    /// Creates a new <see cref="User"/> instance based on the provided user creation data.
    /// </summary>
    /// <param name="data">The user creation data containing the first name and last name for creating the user.</param>
    /// <returns>
    /// A <see cref="Result{TValue}"/> containing the newly created <see cref="User"/> instance if successful,
    /// or an error message if the user creation fails.
    /// </returns>
    public static Result<User> Create(UserCreationData data)
    {
        var nombreUsuarioResult = UsernameServices.GenerateUsername(data.FirstName, data.LastName);
        if (nombreUsuarioResult.IsFailure)
        {
            return Result.Failure<User>(nombreUsuarioResult.Error);
        }

        var user = new User(Guid.NewGuid(), data, nombreUsuarioResult.Value);

        user.AddDomainEvent(new UserCreatedDomainEvent(user.Id));
        return Result.Success(user);
    }
}