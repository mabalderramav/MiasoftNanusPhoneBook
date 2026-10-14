using MiasoftNanus.PhoneBook.Domain.Abstractions;
using MiasoftNanus.PhoneBook.Domain.Agendas.Entities;
using MiasoftNanus.PhoneBook.Domain.Locations.Errors;

namespace MiasoftNanus.PhoneBook.Domain.Locations.Entities;

/// <summary>
/// Represents a location entity within the phone book domain.
/// </summary>
public class Location : Entity
{
    private Location()
    {
        Code = string.Empty;
        Description = string.Empty;
        Agendas = null;
    }

    public required string Code { get; init; }
    public required string Description { get; init; }
    public ICollection<Agenda>? Agendas { get; init; }

    /// <summary>
    /// Creates a new instance of the <see cref="Location"/> class with the specified code and description.
    /// Validates the inputs and returns a result indicating success or failure.
    /// </summary>
    /// <param name="code">The unique code for the location. Cannot be null or whitespace.</param>
    /// <param name="description">The description for the location. Cannot be null or whitespace.</param>
    /// <returns>A <see cref="Result{TValue}"/> containing the created <see cref="Location"/> if successful,
    /// or an error if the inputs are invalid.</returns>
    public static Result<Location> Create(string code, string description)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return Result.Failure<Location>(LocationErrors.CodeCannotBeNullOrWhitespace);
        }

        if (string.IsNullOrWhiteSpace(description))
        {
            return Result.Failure<Location>(LocationErrors.DescriptionCannotBeNullOrWhitespace);
        }

        return Result.Success(new Location()
        {
            Id = Guid.NewGuid(),
            Code = code,
            Description = description
        });
    }
}