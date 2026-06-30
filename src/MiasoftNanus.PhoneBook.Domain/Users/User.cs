using ErrorOr;
using MiasoftNanus.PhoneBook.Domain.Common;
using MiasoftNanus.PhoneBook.Domain.Common.Interfaces;

namespace MiasoftNanus.PhoneBook.Domain.Users;

public class User(
    string firstName,
    string lastName,
    string email,
    string passwordHash,
    string profileId,
    Guid? id,
    bool enabled = true)
    : Entity(id ?? Guid.NewGuid())
{
    public string FirstName { get; } = firstName;
    public string LastName { get; } = lastName;
    public string ProfileId { get; } = profileId;
    public bool Enabled { get; } = enabled;
    public string Email { get; } = email;

    public ErrorOr<bool> IsCorrectPasswordHash(string password, IPasswordHasher passwordHasher)
    {
        if (string.IsNullOrWhiteSpace(password))
            return Error.Validation("Password", "Password cannot be empty.");

        return passwordHasher.IsCorrectPassword(password, passwordHash);
    }
}