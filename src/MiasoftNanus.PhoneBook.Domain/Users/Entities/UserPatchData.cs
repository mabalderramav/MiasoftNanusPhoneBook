using MiasoftNanus.PhoneBook.Domain.Shared.Enums;

namespace MiasoftNanus.PhoneBook.Domain.Users.Entities;

/// <summary>
/// Represents a partial update of the user details. A <c>null</c> member keeps the current value.
/// It does not contain the password.
/// </summary>
public sealed class UserPatchData
{
    public string? FirstName { get; init; }
    public string? LastName { get; init; }
    public DateTime? Birthdate { get; init; }
    public string? Email { get; init; }
    public string? Country { get; init; }
    public string? State { get; init; }
    public string? Province { get; init; }
    public string? District { get; init; }
    public string? Street { get; init; }
    public States? States { get; init; }
    public Guid? ProfileId { get; init; }

    /// <summary>
    /// Gets whether at least one member is set.
    /// </summary>
    public bool HasChanges =>
        FirstName is not null || LastName is not null || Birthdate is not null || Email is not null
        || Country is not null || State is not null || Province is not null || District is not null
        || Street is not null || States is not null || ProfileId is not null;
}
