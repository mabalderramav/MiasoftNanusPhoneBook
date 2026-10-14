using MiasoftNanus.PhoneBook.Domain.Abstractions;
using MiasoftNanus.PhoneBook.Domain.Agendas.Errors;
using MiasoftNanus.PhoneBook.Domain.Agendas.Events;
using MiasoftNanus.PhoneBook.Domain.Locations.Entities;
using MiasoftNanus.PhoneBook.Domain.Shared.ObjectValues;
using MiasoftNanus.PhoneBook.Domain.Users.Entities;
using MiasoftNanus.PhoneBook.Domain.Workstations.Entities;

namespace MiasoftNanus.PhoneBook.Domain.Agendas.Entities;

/// <summary>
/// Represents the Agenda entity which contains details about a contact or person in the phone book domain.
/// </summary>
/// <remarks>
/// The Agenda entity is responsible for storing information such as contact details, workstation,
/// location, and metadata about its creation. It also supports domain events related to its creation.
/// </remarks>
public class Agenda : Entity
{
    private Agenda()
    {
        Code = string.Empty;
        FullName = string.Empty;
        Email = null;
        Enabled = true;
        ImagePath = string.Empty;
        DirectLine = string.Empty;
        Internal = string.Empty;
        Corporate = string.Empty;
        DirectAccess = string.Empty;
        Workstation = null;
        Location = null;
        UserCreated = null;
        WorkstationId = Guid.Empty;
        LocationId = Guid.Empty;
        UserIdCreated = Guid.Empty;
    }

    public required string Code { get; init; }
    public required string FullName { get; init; }
    public Email? Email { get; set; }
    public required bool Enabled { get; init; }
    public string? ImagePath { get; set; }
    public string? DirectLine { get; set; }
    public string? Internal { get; set; }
    public string? Corporate { get; set; }
    public string? DirectAccess { get; set; }
    public Guid? WorkstationId { get; set; }
    public Workstation? Workstation { get; set; }
    public Guid? LocationId { get; set; }
    public Location? Location { get; set; }
    public Guid? UserIdCreated { get; set; }
    public User? UserCreated { get; set; }

    /// <summary>
    /// Creates a new instance of the <see cref="Agenda"/> class based on the provided data.
    /// </summary>
    /// <param name="data">An instance of <see cref="AgendaCreationData"/> containing the properties required
    /// to create an <see cref="Agenda"/>.</param>
    /// <returns>A <see cref="Result{Agenda}"/> representing the outcome of the operation, which either contains
    /// the created <see cref="Agenda"/> or an error.</returns>
    public static Result<Agenda> Create(AgendaCreationData data)
    {
        // Validate the input data to ensure that required fields are not null or empty.
        if (string.IsNullOrWhiteSpace(data.Code))
            return Result.Failure<Agenda>(AgendaErrors.CodeCannotBeNullOrWhitespace);
        if (string.IsNullOrWhiteSpace(data.FullName))
            return Result.Failure<Agenda>(AgendaErrors.FullNameCannotBeNullOrWhitespace);

        // Create a new instance of the Agenda entity using the provided data.
        var agenda = new Agenda
        {
            Code = data.Code,
            FullName = data.FullName,
            Email = data.Email,
            Enabled = data.Enabled,
            ImagePath = data.ImagePath,
            DirectLine = data.DirectLine,
            Internal = data.Internal,
            Corporate = data.Corporate,
            DirectAccess = data.DirectAccess
        };

        // Add domain event for agenda creation.
        agenda.AddDomainEvent(new AgendaCreatedDomainEvent(agenda.Id));

        // Return a successful result containing the created agenda.
        return Result.Success(agenda);
    }
}