using MiasoftNanus.PhoneBook.Domain.Abstractions;
using MiasoftNanus.PhoneBook.Domain.Profiles.Errors;

namespace MiasoftNanus.PhoneBook.Domain.Profiles.Entities;

/// <summary>
/// Represents a profile entity within the domain model.
/// </summary>
/// <remarks>
/// The <c>Profile</c> class encapsulates the details of a user or system profile,
/// including its unique identifier, name, and optional description. It extends the base <c>Entity</c>
/// class, inheriting its properties and behaviors.
/// </remarks>
public class Profile : Entity
{
    private Profile(Guid id, string profileName, string? description)
    {
        Id = id;
        ProfileName = profileName;
        Description = description;
    }

    public string ProfileName { get; private set; }
    public string? Description { get; private set; }

    /// <summary>
    /// Creates a new <see cref="Profile"/> instance with the specified name and description.
    /// </summary>
    /// <param name="profileName">The name of the profile to be created. Cannot be null or whitespace.</param>
    /// <param name="description">The description of the profile. This parameter is optional and can be null.</param>
    /// <returns>
    /// A <see cref="Result{Profile}"/> that contains the created <see cref="Profile"/> instance if successful,
    /// or an error if the profile name is null, whitespace, or invalid.
    /// </returns>
    public static Result<Profile> Create(string profileName, string? description)
    {
        var profile = new Profile(Guid.CreateVersion7(), profileName, description);
        return string.IsNullOrWhiteSpace(profileName)
            ? Result.Failure<Profile>(ProfileErrors.ProfileNameCannotBeNullOrWhitespace)
            : Result.Success(profile);
    }

    /// <summary>
    /// Updates the <see cref="Profile"/> instance with the specified name and description.
    /// </summary>
    /// <param name="profileName">The new name for the profile. Cannot be null or whitespace.</param>
    /// <param name="description">The new description for the profile. This parameter is optional and
    /// can be null.</param>
    /// <returns>
    /// A <see cref="Result"/> indicating the success or failure of the update operation. If unsuccessful,
    /// the result will contain the corresponding error.
    /// </returns>
    public Result Update(string profileName, string? description)
    {
        if (string.IsNullOrWhiteSpace(profileName))
            return Result.Failure(ProfileErrors.ProfileNameCannotBeNullOrWhitespace);

        ProfileName = profileName;
        Description = description;
        return Result.Success();
    }
}