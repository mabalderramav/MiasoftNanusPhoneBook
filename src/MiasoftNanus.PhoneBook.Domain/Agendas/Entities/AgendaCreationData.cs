using MiasoftNanus.PhoneBook.Domain.Shared.ObjectValues;

namespace MiasoftNanus.PhoneBook.Domain.Agendas.Entities;

/// <summary>
/// Represents the data required to create an agenda entry in the system.
/// </summary>
/// <remarks>
/// This record contains all necessary information for creating a new agenda entry, such as
/// identification, contact information, and accessibility attributes. The properties in
/// this record provide flexibility for detailed agenda creation while maintaining immutability.
/// </remarks>
public record AgendaCreationData(
    string Code,
    string FullName,
    Email? Email,
    bool Enabled,
    string? ImagePath,
    string? DirectLine,
    string? Internal,
    string? Corporate,
    string? DirectAccess);