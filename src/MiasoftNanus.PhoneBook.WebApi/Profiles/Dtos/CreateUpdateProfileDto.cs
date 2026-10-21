namespace MiasoftNanus.PhoneBook.WebApi.Profiles.Dtos;

/// <summary>
/// Data Transfer Object used for creating or updating a profile.
/// </summary>
public record CreateUpdateProfileDto(
    string ProfileName,
    string? Description
);