using MiasoftNanus.PhoneBook.Domain.Abstractions;
using MiasoftNanus.PhoneBook.Domain.Users.Errors;

namespace MiasoftNanus.PhoneBook.Domain.Users.ObjectValues;

/// <summary>
/// Represents the password as a value object used in the domain.
/// </summary>
/// <remarks>
/// The <c>Password</c> record encapsulates the logic for creating, validating,
/// and storing a password in a consistent and immutable manner.
/// </remarks>
/// <example>
/// Validation rules:
/// 1. The password value must not be null or whitespace.
/// 2. The password must have a minimum length of 8 characters.
/// </example>
/// <seealso cref="Result"/>
/// <seealso cref="Error"/>
public record Password
{
    /// <summary>
    /// Gets the value of the password.
    /// </summary>
    /// <remarks>
    /// The Value property holds the string representation of the password.
    /// It is guaranteed to be non-null and meet the validation criteria defined by the domain,
    /// such as minimum length and absence of whitespace.
    /// </remarks>
    public string Value { get; init; }

    private Password(string value)
    {
        Value = value;
    }

    /// <summary>
    /// Creates a new <see cref="Password"/> instance based on the provided value.
    /// </summary>
    /// <param name="value">The password value to be evaluated and encapsulated within the <see cref="Password"/>
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

        return value.Length < 8
            ? Result.Failure<Password>(UserErrors.InvalidPassword)
            : Result.Success(new Password(value));
    }
}