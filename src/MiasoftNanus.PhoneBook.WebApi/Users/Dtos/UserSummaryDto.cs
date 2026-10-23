namespace MiasoftNanus.PhoneBook.WebApi.Users.Dtos;

/// <summary>
/// Minimal, non-sensitive view of a user, used when a user is embedded in another resource.
/// </summary>
public record UserSummaryDto(
    Guid Id,
    string UserName,
    string FirstName,
    string LastName
);
