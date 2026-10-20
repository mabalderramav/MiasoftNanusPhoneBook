using MiasoftNanus.PhoneBook.Application.Abstractions.CQRS;
using MiasoftNanus.PhoneBook.Domain.Abstractions;
using MiasoftNanus.PhoneBook.Domain.Profiles.Entities;

namespace MiasoftNanus.PhoneBook.Application.Profiles.UseCases.Commands.CreateProfile;

/// <summary>
/// Represents a command to create a new profile in the phone book.
/// </summary>
/// 
/// <remarks>
/// The command encapsulates the necessary data for creating a profile,
/// including the profile name and an optional description. It is designed
/// to be used in conjunction with a command handler.
/// </remarks>
/// 
/// <param name="ProfileName">
/// The name of the profile to be created. This field is mandatory and
/// must not be null or empty.
/// </param>
/// 
/// <param name="Description">
/// An optional description of the profile. This field can be null if
/// no specific description is provided.
/// </param>
/// 
/// <returns>
/// A result containing the created profile entity or an error,
/// depending on the execution outcome.
/// </returns>
public record CreateProfileCommand(
    string ProfileName,
    string? Description
) : ICommand<Result<Profile>>;