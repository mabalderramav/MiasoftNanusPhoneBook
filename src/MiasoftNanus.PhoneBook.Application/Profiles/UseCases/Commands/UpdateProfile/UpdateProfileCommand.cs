using MiasoftNanus.PhoneBook.Application.Abstractions.CQRS;
using MiasoftNanus.PhoneBook.Domain.Abstractions;
using MiasoftNanus.PhoneBook.Domain.Profiles.Entities;

namespace MiasoftNanus.PhoneBook.Application.Profiles.UseCases.Commands.UpdateProfile;

/// <summary>
/// Represents a command to update an existing profile in the phone book.
/// </summary>
///
/// <remarks>
/// The command encapsulates the necessary data for updating a profile,
/// including its unique identifier, the new profile name and description.
/// It is designed to be used in conjunction with a command handler.
/// </remarks>
///
/// <param name="Id">
/// The unique identifier of the profile to be updated.
/// </param>
///
/// <param name="ProfileName">
/// The new name of the profile. This field is mandatory and
/// must not be null or empty.
/// </param>
///
/// <param name="Description">
/// The new description of the profile. This field must not be
/// null or whitespace.
/// </param>
///
/// <returns>
/// A result containing the updated profile entity or an error,
/// depending on the execution outcome.
/// </returns>
public record UpdateProfileCommand(
    Guid Id,
    string ProfileName,
    string? Description
) : ICommand<Result<Profile>>;
