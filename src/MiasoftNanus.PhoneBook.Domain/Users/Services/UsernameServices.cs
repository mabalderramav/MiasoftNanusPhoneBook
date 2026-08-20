using MiasoftNanus.PhoneBook.Domain.Abstractions;
using MiasoftNanus.PhoneBook.Domain.Users.ObjectValues;

namespace MiasoftNanus.PhoneBook.Domain.Users.Services;

/// <summary>
/// Provides services for generating and managing usernames within the domain.
/// </summary>
/// <remarks>
/// This class includes methods that encapsulate business logic for creating and
/// validating usernames, ensuring adherence to established domain rules and
/// constraints.
/// </remarks>
public static class UsernameServices
{
    /// <summary>
    /// Generates a username based on the provided first and last name.
    /// </summary>
    /// <param name="firstName">
    /// The first name of the user. This parameter must not be null, empty, or consist solely of whitespace.
    /// </param>
    /// <param name="lastName">
    /// The last name of the user. This parameter must not be null, empty, or consist solely of whitespace.
    /// </param>
    /// <returns>
    /// A <see cref="Result{TValue}"/> containing the generated <see cref="Username"/> if the operation is successful;
    /// otherwise, a failure result with the associated error.
    /// </returns>
    public static Result<Username> GenerateUsername(string firstName, string lastName)
    {
        if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName))
        {
            return Result.Failure<Username>(Error.NullValue);
        }

        var nombreUsuario = $"{firstName.ToUpper()[..1]}.{lastName.ToUpper()}";
        return Username.Create(nombreUsuario);
    }
}