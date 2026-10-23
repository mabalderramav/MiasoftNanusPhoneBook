using MiasoftNanus.PhoneBook.Domain.Abstractions;
using MiasoftNanus.PhoneBook.Domain.Agendas.Entities;
using MiasoftNanus.PhoneBook.Domain.Profiles.Entities;
using MiasoftNanus.PhoneBook.Domain.Shared.Enums;
using MiasoftNanus.PhoneBook.Domain.Users.Errors;
using MiasoftNanus.PhoneBook.Domain.Users.Events;
using MiasoftNanus.PhoneBook.Domain.Users.ObjectValues;
using MiasoftNanus.PhoneBook.Domain.Users.Services;
using Email = MiasoftNanus.PhoneBook.Domain.Shared.ObjectValues.Email;

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
/// methods for user creation, update, and event handling.
/// </example>
public class User : Entity
{
    // Required by EF Core: the owned Address cannot be bound through constructor parameters.
    private User()
    {
    }

    public string? FirstName { get; private set; }
    public string? LastName { get; private set; }
    public Password? Password { get; private set; }
    public Username? UserName { get; private set; }
    public DateTime Birthdate { get; private set; }
    public Email? Email { get; private set; }
    public Address? Address { get; private set; }
    public States? States { get; private set; }
    public DateTime DateOfLastChange { get; private set; }
    public Guid ProfileId { get; private set; }
    public Profile? Profile { get; init; }
    public ICollection<Agenda>? Agendas { get; init; }

    /// <summary>
    /// Creates a new <see cref="User"/> instance based on the provided user creation data.
    /// </summary>
    /// <param name="data">The data used to create the user. The username is generated from the first and
    /// last name.</param>
    /// <returns>
    /// A <see cref="Result{TValue}"/> containing the newly created <see cref="User"/> instance if successful,
    /// or an error if the user creation fails.
    /// </returns>
    public static Result<User> Create(UserCreationData data)
    {
        var validationResult = Validate(data);
        if (validationResult.IsFailure)
            return Result.Failure<User>(validationResult.Error);

        var passwordResult = Password.Create(data.Password);
        if (passwordResult.IsFailure)
            return Result.Failure<User>(passwordResult.Error);

        var usernameResult = UsernameServices.GenerateUsername(data.FirstName, data.LastName);
        if (usernameResult.IsFailure)
            return Result.Failure<User>(usernameResult.Error);

        var user = new User
        {
            Id = Guid.CreateVersion7(),
            UserName = usernameResult.Value,
            Password = passwordResult.Value
        };
        user.Apply(data);

        user.AddDomainEvent(new UserCreatedDomainEvent(user.Id));
        return Result.Success(user);
    }

    /// <summary>
    /// Updates the details of this user after validating the provided data. The username and the password are
    /// not changed; use <see cref="ChangePassword"/> for the password.
    /// </summary>
    public Result Update(UserData data)
    {
        var validationResult = Validate(data);
        if (validationResult.IsFailure)
            return validationResult;

        Apply(data);
        return Result.Success();
    }

    /// <summary>
    /// Applies a partial update: every member of <paramref name="data"/> that is set replaces the current value, the
    /// others are kept. The merged details are validated like <see cref="Update"/>. The username and the password are
    /// not changed.
    /// </summary>
    /// <param name="data">The details to change.</param>
    /// <param name="dateOfLastChange">The date of the change.</param>
    public Result Patch(UserPatchData data, DateTime dateOfLastChange)
    {
        var address = Address!;
        return Update(new UserData
        {
            FirstName = data.FirstName ?? FirstName!,
            LastName = data.LastName ?? LastName!,
            Birthdate = data.Birthdate ?? Birthdate,
            Email = data.Email ?? Email!.Value,
            Address = new Address(
                data.Country ?? address.Country,
                data.State ?? address.State,
                data.Province ?? address.Province,
                data.District ?? address.District,
                data.Street ?? address.Street),
            States = data.States ?? States!.Value,
            DateOfLastChange = dateOfLastChange,
            ProfileId = data.ProfileId ?? ProfileId
        });
    }

    /// <summary>
    /// Validates the raw password, replaces the current one with its hash and records the change date.
    /// </summary>
    /// <param name="password">The new raw password.</param>
    /// <param name="passwordHasher">The hasher used to hash the new password.</param>
    /// <param name="dateOfLastChange">The date of the change.</param>
    public Result ChangePassword(string password, IPasswordHasher passwordHasher, DateTime dateOfLastChange)
    {
        var passwordResult = Password.Create(password);
        if (passwordResult.IsFailure)
            return Result.Failure(passwordResult.Error);

        Password = passwordResult.Value.Hash(passwordHasher);
        DateOfLastChange = dateOfLastChange;
        return Result.Success();
    }

    /// <summary>
    /// Replaces the plain password with its hash. Must be called before the user is persisted.
    /// </summary>
    /// <param name="passwordHasher">The hasher used to hash the plain password.</param>
    public void HashPassword(IPasswordHasher passwordHasher)
    {
        Password = Password!.Hash(passwordHasher);
    }

    private void Apply(UserData data)
    {
        FirstName = data.FirstName;
        LastName = data.LastName;
        // A birthdate is a calendar date: keep it without timezone shifting, as UTC for timestamp.
        Birthdate = DateTime.SpecifyKind(data.Birthdate.Date, DateTimeKind.Utc);
        Email = Email.Create(data.Email).Value;
        Address = data.Address;
        States = data.States;
        DateOfLastChange = data.DateOfLastChange;
        ProfileId = data.ProfileId;
    }

    private static Result Validate(UserData data)
    {
        if (string.IsNullOrWhiteSpace(data.FirstName))
            return Result.Failure(UserErrors.FirstNameCannotBeNullOrWhitespace);

        if (string.IsNullOrWhiteSpace(data.LastName))
            return Result.Failure(UserErrors.LastNameCannotBeNullOrWhitespace);

        var emailResult = Email.Create(data.Email);
        if (emailResult.IsFailure)
            return Result.Failure(emailResult.Error);

        if (data.Birthdate == default)
            return Result.Failure(UserErrors.BirthdateIsRequired);

        if (string.IsNullOrWhiteSpace(data.Address.Country)
            || string.IsNullOrWhiteSpace(data.Address.State)
            || string.IsNullOrWhiteSpace(data.Address.Province)
            || string.IsNullOrWhiteSpace(data.Address.District)
            || string.IsNullOrWhiteSpace(data.Address.Street))
            return Result.Failure(UserErrors.AddressIsIncomplete);

        if (!Enum.IsDefined(data.States))
            return Result.Failure(UserErrors.InvalidState);

        if (data.ProfileId == Guid.Empty)
            return Result.Failure(UserErrors.ProfileIdCannotBeEmpty);

        return Result.Success();
    }
}
