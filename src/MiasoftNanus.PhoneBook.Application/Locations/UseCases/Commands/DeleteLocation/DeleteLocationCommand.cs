using MiasoftNanus.PhoneBook.Application.Abstractions.CQRS;
using MiasoftNanus.PhoneBook.Domain.Abstractions;

namespace MiasoftNanus.PhoneBook.Application.Locations.UseCases.Commands.DeleteLocation;

/// <summary>
/// Represents a command to delete an existing location from the phone book.
/// </summary>
///
/// <remarks>
/// The command encapsulates the necessary data for deleting a location,
/// which is its unique identifier. It is designed to be used in
/// conjunction with a command handler.
/// </remarks>
///
/// <param name="Id">
/// The unique identifier of the location to be deleted.
/// </param>
///
/// <returns>
/// A result indicating the success or failure of the delete operation.
/// </returns>
public record DeleteLocationCommand(Guid Id) : ICommand<Result>;
