using MiasoftNanus.PhoneBook.Application.Abstractions.CQRS;
using MiasoftNanus.PhoneBook.Domain.Abstractions;

namespace MiasoftNanus.PhoneBook.Application.Profiles.UseCases.Commands.DeleteProfile;

/// <summary>
/// Represents a command to delete an existing profile from the phone book.
/// </summary>
///
/// <remarks>
/// The command encapsulates the necessary data for deleting a profile,
/// which is its unique identifier. It is designed to be used in
/// conjunction with a command handler.
/// </remarks>
///
/// <param name="Id">
/// The unique identifier of the profile to be deleted.
/// </param>
///
/// <returns>
/// A result indicating the success or failure of the delete operation.
/// </returns>
public record DeleteProfileCommand(Guid Id) : ICommand<Result>;