using MiasoftNanus.PhoneBook.Domain.Abstractions;
using MiasoftNanus.PhoneBook.Domain.Users.Errors;
using System.Text.RegularExpressions;

namespace MiasoftNanus.PhoneBook.Domain.Users.ObjectValues;

/// <summary>
/// Represents an immutable value object for an email address.
/// </summary>
/// <remarks>
/// The <see cref="Email"/> type is used to encapsulate the value of an email address
/// and validate its format. It ensures that the email address adheres to a specific
/// pattern defined internally.
/// </remarks>
public partial record Email
{
    // Deliberately validates a practical email shape; it does not attempt full RFC 5322 parsing.
    [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.CultureInvariant)]
    private static partial Regex EmailPattern();

    /// <summary>
    /// Gets the email address value represented by this instance.
    /// </summary>
    /// <remarks>
    /// The <c>Value</c> property contains a string representation of the email address
    /// and is assigned during the creation of the <see cref="Email"/> object.
    /// It is guaranteed to be valid if the object is successfully constructed using the <c>Create</c> method.
    /// </remarks>
    public string Value { get; init; }

    private Email(string value)
    {
        Value = value;
    }

    /// <summary>
    /// Creates a new <see cref="Email"/> instance if the provided email value is valid.
    /// </summary>
    /// <param name="value">The email address to be validated and encapsulated within an <see cref="Email"/>
    /// instance.</param>
    /// <returns>
    /// A <see cref="Result{TValue}"/> containing an <see cref="Email"/> instance if the operation succeeds,
    /// or an error if the provided value is null, empty, or invalid.
    /// </returns>
    public static Result<Email> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Result.Failure<Email>(Error.NullValue);
        }

        return !EmailPattern().IsMatch(value) ? Result.Failure<Email>(UserErrors.InvalidEmail) 
            : Result.Success(new Email(value));
    }
}
