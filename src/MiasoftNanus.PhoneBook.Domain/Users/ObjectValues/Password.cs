using MiasoftNanus.PhoneBook.Domain.Abstractions;
using MiasoftNanus.PhoneBook.Domain.Users.Errors;
using MiasoftNanus.PhoneBook.Domain.Users.Services;

namespace MiasoftNanus.PhoneBook.Domain.Users.ObjectValues;

/// <summary>
/// Represents the password as a value object used in the domain.
/// </summary>
/// <remarks>
/// The <c>Password</c> record encapsulates the logic for creating, validating,
/// and hashing a password in a consistent and immutable manner. A password created from raw input
/// is plain (validated, not hashed yet) and only lives in memory; a password created by <see cref="Hash"/>
/// or rehydrated by <see cref="FromHash"/> holds the hash and is the only form that can be persisted.
/// </remarks>
/// <example>
/// Validation rules (applied on the raw input before hashing):
/// 1. The password value must not be null or whitespace.
/// 2. The password must have a minimum length of 8 characters.
/// </example>
/// <seealso cref="Result"/>
/// <seealso cref="Error"/>
public sealed record Password
{
    private const int MinLength = 8;

    /// <summary>
    /// Gets the value of the password.
    /// </summary>
    /// <remarks>
    /// Holds the raw password while <see cref="IsHashed"/> is <c>false</c>, and the hash otherwise.
    /// </remarks>
    public string Value { get; }

    /// <summary>
    /// Gets a value indicating whether <see cref="Value"/> holds a hash.
    /// </summary>
    public bool IsHashed { get; }

    private Password(string value, bool isHashed)
    {
        Value = value;
        IsHashed = isHashed;
    }

    /// <summary>
    /// Creates a new plain <see cref="Password"/> instance based on the provided raw value.
    /// </summary>
    /// <param name="value">The raw password value to be evaluated and encapsulated within the <see cref="Password"/>
    /// object. This value must not be null, whitespace, or shorter than the defined minimum length.</param>
    /// <returns>
    /// A <see cref="Result{TValue}"/> containing a successfully created <see cref="Password"/> if the input is valid;
    /// otherwise, a failure <see cref="Result{TValue}"/> containing the appropriate <see cref="Error"/>
    /// describing why the password is invalid.
    /// </returns>
    public static Result<Password> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Result.Failure<Password>(Error.NullValue);
        }

        return value.Length < MinLength
            ? Result.Failure<Password>(UserErrors.InvalidPassword)
            : Result.Success(new Password(value, isHashed: false));
    }

    /// <summary>
    /// Rehydrates a stored password hash.
    /// </summary>
    /// <param name="hash">The stored password hash.</param>
    /// <returns>A hashed <see cref="Password"/> instance.</returns>
    public static Password FromHash(string hash) => new(hash, isHashed: true);

    /// <summary>
    /// Returns the hashed form of this password. A password that is already hashed is returned as is.
    /// </summary>
    /// <param name="passwordHasher">The hasher used to hash the raw value.</param>
    /// <returns>A hashed <see cref="Password"/> instance.</returns>
    public Password Hash(IPasswordHasher passwordHasher) =>
        IsHashed ? this : new Password(passwordHasher.Hash(Value), isHashed: true);

    /// <summary>
    /// Returns a masked representation so the password never leaks through logs.
    /// </summary>
    public override string ToString() => "***";
}
