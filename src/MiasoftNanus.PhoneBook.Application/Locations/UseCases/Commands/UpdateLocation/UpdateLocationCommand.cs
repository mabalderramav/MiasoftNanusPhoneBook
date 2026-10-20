using MiasoftNanus.PhoneBook.Application.Abstractions.CQRS;
using MiasoftNanus.PhoneBook.Domain.Abstractions;
using MiasoftNanus.PhoneBook.Domain.Locations.Entities;

namespace MiasoftNanus.PhoneBook.Application.Locations.UseCases.Commands.UpdateLocation;

/// <summary>
/// Represents a command to update an existing location in the phone book.
/// </summary>
///
/// <remarks>
/// The command encapsulates the necessary data for updating a location,
/// including its unique identifier, the new code and description.
/// It is designed to be used in conjunction with a command handler.
/// </remarks>
///
/// <param name="Id">
/// The unique identifier of the location to be updated.
/// </param>
///
/// <param name="Code">
/// The new code of the location. This field is mandatory and
/// must not be null or whitespace.
/// </param>
///
/// <param name="Description">
/// The new description of the location. This field is mandatory and
/// must not be null or whitespace.
/// </param>
///
/// <returns>
/// A result containing the updated location entity or an error,
/// depending on the execution outcome.
/// </returns>
public record UpdateLocationCommand(
    Guid Id,
    string Code,
    string Description
) : ICommand<Result<Location>>;
