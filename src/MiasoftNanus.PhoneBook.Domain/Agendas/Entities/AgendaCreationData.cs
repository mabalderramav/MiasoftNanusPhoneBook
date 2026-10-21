namespace MiasoftNanus.PhoneBook.Domain.Agendas.Entities;

/// <summary>
/// Represents the data required to create or update an Agenda instance.
/// </summary>
/// <remarks>
/// This class serves as a container for the raw agenda-related values. Value objects such as
/// <see cref="Shared.ObjectValues.Email"/> are validated and built by the <see cref="Agenda"/> entity itself.
/// </remarks>
public sealed class AgendaCreationData
{
    public required string Code { get; init; }
    public required string FullName { get; init; }
    public required string Email { get; init; }
    public required bool Enabled { get; init; }
    public string? ImagePath { get; init; }
    public string? DirectLine { get; init; }
    public string? Internal { get; init; }
    public string? Corporate { get; init; }
    public string? DirectAccess { get; init; }
    public required Guid WorkstationId { get; init; }
    public required Guid LocationId { get; init; }
    public required Guid UserIdCreated { get; init; }
}
