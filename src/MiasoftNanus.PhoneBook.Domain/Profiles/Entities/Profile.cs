using MiasoftNanus.PhoneBook.Domain.Abstractions;

namespace MiasoftNanus.PhoneBook.Domain.Profiles.Entities;

/// <summary>
/// Represents a profile entity within the domain model.
/// </summary>
/// <remarks>
/// The <c>Profile</c> class encapsulates the details of a user or system profile,
/// including its unique identifier, name, and optional description. It extends the base <c>Entity</c>
/// class, inheriting its properties and behaviors.
/// </remarks>
public class Profile(Guid id, string? profileName, string? description) : Entity(id)
{
    public string? ProfileName { get; private set; } = profileName;
    public string? Description { get; private set; } = description;
}