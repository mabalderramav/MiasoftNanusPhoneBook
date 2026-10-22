namespace MiasoftNanus.PhoneBook.Domain.Users.Services;

/// <summary>
/// Hashes and verifies user passwords.
/// </summary>
/// <remarks>
/// Implemented in the infrastructure layer so the domain does not depend on a concrete hashing algorithm.
/// </remarks>
public interface IPasswordHasher
{
    /// <summary>
    /// Hashes the provided plain-text password.
    /// </summary>
    /// <param name="password">The plain-text password.</param>
    /// <returns>The hashed password, including its salt and algorithm metadata.</returns>
    string Hash(string password);

    /// <summary>
    /// Verifies a plain-text password against a stored hash.
    /// </summary>
    /// <param name="password">The plain-text password.</param>
    /// <param name="passwordHash">The stored password hash.</param>
    /// <returns><c>true</c> if the password matches the hash; otherwise, <c>false</c>.</returns>
    bool Verify(string password, string passwordHash);
}
