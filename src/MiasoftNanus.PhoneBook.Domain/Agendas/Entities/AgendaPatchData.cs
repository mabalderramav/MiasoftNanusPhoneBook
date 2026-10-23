namespace MiasoftNanus.PhoneBook.Domain.Agendas.Entities;

/// <summary>
/// Represents a partial update of an agenda. A <c>null</c> member keeps the current value. For the optional
/// phone / image members, an empty or blank string clears the value. The creator user is not patchable.
/// </summary>
public sealed class AgendaPatchData
{
    public string? Code { get; init; }
    public string? FullName { get; init; }
    public string? Email { get; init; }
    public bool? Enabled { get; init; }
    public string? ImagePath { get; init; }
    public string? DirectLine { get; init; }
    public string? Internal { get; init; }
    public string? Corporate { get; init; }
    public string? DirectAccess { get; init; }
    public Guid? WorkstationId { get; init; }
    public Guid? LocationId { get; init; }

    /// <summary>
    /// Gets whether at least one member is set.
    /// </summary>
    public bool HasChanges =>
        Code is not null || FullName is not null || Email is not null || Enabled is not null
        || ImagePath is not null || DirectLine is not null || Internal is not null || Corporate is not null
        || DirectAccess is not null || WorkstationId is not null || LocationId is not null;
}
