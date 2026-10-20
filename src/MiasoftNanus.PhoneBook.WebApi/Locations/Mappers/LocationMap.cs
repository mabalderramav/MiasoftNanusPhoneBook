using MiasoftNanus.PhoneBook.Domain.Abstractions;
using MiasoftNanus.PhoneBook.Domain.Locations.Entities;
using MiasoftNanus.PhoneBook.WebApi.Locations.Dtos;

namespace MiasoftNanus.PhoneBook.WebApi.Locations.Mappers;

/// <summary>
/// Provides mapping functionality for the <c>Location</c> entity and related data transfer objects (DTOs).
/// </summary>
/// <remarks>
/// The <c>LocationMap</c> class includes extension methods that facilitate the conversion between
/// <c>Location</c> domain entities and <c>LocationDto</c> objects. These mappings are useful for transferring
/// location data between different layers of the application, such as between the domain layer and
/// the presentation layer in a Web API context.
/// </remarks>
public static class LocationMap
{
    /// <summary>
    /// Converts an instance of <see cref="Location"/> to an instance of <see cref="LocationDto"/>.
    /// </summary>
    /// <param name="location">The <see cref="Location"/> entity to be converted to a DTO.</param>
    /// <returns>A new <see cref="LocationDto"/> containing the data from the <see cref="Location"/> entity.</returns>
    public static LocationDto ToDto(this Location location)
    {
        return new LocationDto(
            location.Id,
            location.Code,
            location.Description
        );
    }

    /// <summary>
    /// Converts a collection of <see cref="Location"/> to a collection of <see cref="LocationDto"/>.
    /// </summary>
    /// <param name="locations">The <see cref="Location"/> entities to be converted into DTOs.</param>
    /// <returns>A collection of <see cref="LocationDto"/> containing the data from the given
    /// <see cref="Location"/> entities.</returns>
    public static IEnumerable<LocationDto> ToDto(this IEnumerable<Location> locations)
    {
        return locations.Select(location => location.ToDto());
    }

    /// <summary>
    /// Converts an instance of <see cref="CreateUpdateLocationDto"/> to an instance of <see cref="Location"/>.
    /// </summary>
    /// <param name="createUpdateLocationDto">The DTO containing the data for creating or updating a location.</param>
    /// <returns>A <see cref="Result{TValue}"/> containing the new <see cref="Location"/>, or the validation
    /// error if the DTO data is invalid.</returns>
    public static Result<Location> ToEntity(this CreateUpdateLocationDto createUpdateLocationDto)
    {
        return Location.Create(createUpdateLocationDto.Code, createUpdateLocationDto.Description);
    }
}
