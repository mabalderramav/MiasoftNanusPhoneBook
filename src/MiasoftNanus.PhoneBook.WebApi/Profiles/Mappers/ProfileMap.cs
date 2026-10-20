using MiasoftNanus.PhoneBook.Domain.Profiles.Entities;
using MiasoftNanus.PhoneBook.WebApi.Profiles.Dtos;

namespace MiasoftNanus.PhoneBook.WebApi.Profiles.Mappers;

/// <summary>
/// Provides mapping functionality for the <c>Profile</c> entity and related data transfer objects (DTOs).
/// </summary>
/// <remarks>
/// The <c>ProfileMap</c> class includes extension methods that facilitate the conversion between
/// <c>Profile</c> domain entities and <c>ProfileDto</c> objects. These mappings are useful for transferring
/// profile data between different layers of the application, such as between the domain layer and
/// the presentation layer in a Web API context.
/// </remarks>
public static class ProfileMap
{
    /// <summary>
    /// Converts an instance of <see cref="Profile"/> to an instance of <see cref="ProfileDto"/>.
    /// </summary>
    /// <param name="profile">The <see cref="Profile"/> entity to be converted to a DTO.</param>
    /// <returns>A new <see cref="ProfileDto"/> containing the data from the <see cref="Profile"/> entity.</returns>
    public static ProfileDto ToDto(this Profile profile)
    {
        return new ProfileDto(
            profile.Id,
            profile.ProfileName,
            profile.Description
        );
    }

    /// <summary>
    /// Converts an instance of <see cref="Profile"/> to an instance of <see cref="ProfileDto"/>.
    /// </summary>
    /// <param name="profiles">The <see cref="Profile"/> entities to be converted into DTOs.</param>
    /// <returns>A new <see cref="ProfileDto"/> containing the data from the given <see cref="Profile"/>
    /// entities.</returns>
    public static IEnumerable<ProfileDto> ToDto(this IEnumerable<Profile> profiles)
    {
        return profiles.Select(profile => profile.ToDto());
    }

    /// <summary>
    /// Converts an instance of <see cref="CrearUpdateProfileDto"/> to an instance of <see cref="Profile"/>.
    /// </summary>
    /// <param name="crearUpdateProfileDto">The DTO containing the data for creating a new profile.</param>
    /// <returns>A new instance of <see cref="Profile"/> containing the data from the DTO.</returns>
    public static Profile ToEntity(this CrearUpdateProfileDto crearUpdateProfileDto)
    {
        return Profile.Create(crearUpdateProfileDto.ProfileName, crearUpdateProfileDto.Description).Value;
    }
}