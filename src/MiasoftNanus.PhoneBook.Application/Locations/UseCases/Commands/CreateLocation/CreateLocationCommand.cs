using MiasoftNanus.PhoneBook.Application.Abstractions.CQRS;
using MiasoftNanus.PhoneBook.Domain.Abstractions;
using MiasoftNanus.PhoneBook.Domain.Locations.Entities;

namespace MiasoftNanus.PhoneBook.Application.Locations.UseCases.Commands.CreateLocation;

/// <summary>
/// Represents a command to create a new location in the phone book.
/// </summary>
///
/// <remarks>
/// The command encapsulates the necessary data for creating a location,
/// including its code and description. It is designed to be used in
/// conjunction with a command handler.
/// </remarks>
///
/// <param name="Code">
/// The code of the location to be created. This field is mandatory and
/// must not be null or whitespace.
/// </param>
///
/// <param name="Description">
/// The description of the location. This field is mandatory and
/// must not be null or whitespace.
/// </param>
///
/// <returns>
/// A result containing the created location entity or an error,
/// depending on the execution outcome.
/// </returns>
public record CreateLocationCommand(
    string Code,
    string Description
) : ICommand<Result<Location>>;
