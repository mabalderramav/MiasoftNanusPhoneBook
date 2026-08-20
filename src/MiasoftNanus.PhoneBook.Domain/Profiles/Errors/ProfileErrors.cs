using MiasoftNanus.PhoneBook.Domain.Abstractions;

namespace MiasoftNanus.PhoneBook.Domain.Profiles.Errors;

/// <summary>
/// Provides predefined error instances related to the Profile domain.
/// This class centralizes common errors that can occur while working with profiles,
/// making them easy to access and standardize across the application.
/// </summary>
public static class ProfileErrors
{
    public static Error ProfileNotFound =>
        new Error("ProfileErrors.ProfileNotFound", "The profile was not found."
        );
}