using MiasoftNanus.PhoneBook.Domain.Users.Entities;
using MiasoftNanus.PhoneBook.Domain.Users.Services;
using Microsoft.AspNetCore.Identity;

namespace MiasoftNanus.PhoneBook.Infrastructure.Abstractions.Security;

/// <summary>
/// Implements the <see cref="IPasswordHasher"/> interface using ASP.NET Core Identity's
/// PBKDF2-based <see cref="PasswordHasher{TUser}"/>.
/// </summary>
/// <remarks>
/// The produced hashes use the Identity v3 format, which embeds the algorithm, iteration count and salt,
/// so they can be verified and upgraded later without extra columns.
/// </remarks>
internal sealed class PasswordHasher : IPasswordHasher
{
    private readonly PasswordHasher<User> _hasher = new();

    // The default PasswordHasher<TUser> implementation does not use the user instance.
    public string Hash(string password) => _hasher.HashPassword(null!, password);

    public bool Verify(string password, string passwordHash) =>
        _hasher.VerifyHashedPassword(null!, passwordHash, password) != PasswordVerificationResult.Failed;
}
