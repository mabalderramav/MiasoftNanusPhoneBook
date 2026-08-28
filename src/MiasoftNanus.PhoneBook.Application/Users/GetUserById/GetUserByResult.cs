using MiasoftNanus.PhoneBook.Domain.Shared;

namespace MiasoftNanus.PhoneBook.Application.Users.GetUserById;

/// <summary>
/// Represents the result of retrieving a user by their unique identifier.
/// </summary>
/// <remarks>
/// This record encapsulates detailed information about a user, including their personal
/// and geographical details, account metadata, and role information.
/// It is used as the return type for queries related to user retrieval.
/// </remarks>
public sealed record GetUserByResult(
    Guid Id,
    string FirstName,
    string LastName,
    string Username,
    DateTime Birthdate,
    string Email,
    string Country,
    string State,
    string Province,
    string District,
    string Street,
    States? States,
    string RoleName,
    DateTime DateOfLastChange
);