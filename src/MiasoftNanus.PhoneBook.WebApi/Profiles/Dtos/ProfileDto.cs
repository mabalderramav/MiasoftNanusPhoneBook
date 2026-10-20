namespace MiasoftNanus.PhoneBook.WebApi.Profiles.Dtos;

/// <summary>
/// Data transfer object representing a user profile.
/// </summary>
public record ProfileDto(
    Guid Id,
    string ProfileName,
    string? Description
);