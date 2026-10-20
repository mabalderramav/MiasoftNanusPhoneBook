using MiasoftNanus.PhoneBook.Domain.Abstractions;
using MiasoftNanus.PhoneBook.Domain.Agendas.Entities;
using MiasoftNanus.PhoneBook.Domain.Locations.Errors;

namespace MiasoftNanus.PhoneBook.Domain.Locations.Entities;

/// <summary>
/// Represents a location entity within the phone book domain.
/// </summary>
public class Location : Entity
{
    private Location(Guid id, string code, string description)
    {
        Id = id;
        Code = code;
        Description = description;
    }

    public string Code { get; private set; }
    public string Description { get; private set; }
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
        var validationResult = Validate(code, description);
        return validationResult.IsFailure
            ? Result.Failure<Location>(validationResult.Error)
            : Result.Success(new Location(Guid.CreateVersion7(), code, description));
    }

    /// <summary>
    /// Updates the <see cref="Location"/> instance with the specified code and description.
    /// </summary>
    /// <param name="code">The new code for the location. Cannot be null or whitespace.</param>
    /// <param name="description">The new description for the location. Cannot be null or whitespace.</param>
    /// <returns>
    /// A <see cref="Result"/> indicating the success or failure of the update operation. If unsuccessful,
    /// the result will contain the corresponding error.
    /// </returns>
    public Result Update(string code, string description)
    {
        var validationResult = Validate(code, description);
        if (validationResult.IsFailure)
            return validationResult;

        Code = code;
        Description = description;
        return Result.Success();
    }

    private static Result Validate(string code, string description)
    {
        if (string.IsNullOrWhiteSpace(code))
            return Result.Failure(LocationErrors.CodeCannotBeNullOrWhitespace);

        if (string.IsNullOrWhiteSpace(description))
            return Result.Failure(LocationErrors.DescriptionCannotBeNullOrWhitespace);

        return Result.Success();
    }
}
