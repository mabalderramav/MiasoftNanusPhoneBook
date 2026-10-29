﻿using MiasoftNanus.PhoneBook.Domain.Abstractions;
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
    }

    public string Code { get; private set; } = string.Empty;
    public string FullName { get; private set; } = string.Empty;
    public Email? Email { get; private set; }
    public bool Enabled { get; private set; }
    public string? ImagePath { get; private set; }
    public string? DirectLine { get; private set; }
    public string? Internal { get; private set; }
    public string? Corporate { get; private set; }
    public string? DirectAccess { get; private set; }
    public Guid WorkstationId { get; private set; }
    public Workstation? Workstation { get; init; }
    public Guid LocationId { get; private set; }
    public Location? Location { get; init; }
    public Guid UserIdCreated { get; private set; }
    public User? UserCreated { get; init; }

    /// <summary>
    /// Creates a new instance of the <see cref="Agenda"/> class based on the provided data.
    /// </summary>
    /// <param name="data">An instance of <see cref="AgendaCreationData"/> containing the properties required
    /// to create an <see cref="Agenda"/>.</param>
    /// <returns>A <see cref="Result{Agenda}"/> representing the outcome of the operation, which either contains
    /// the created <see cref="Agenda"/> or an error.</returns>
    public static Result<Agenda> Create(AgendaCreationData data)
    {
        var validationResult = Validate(data);
        if (validationResult.IsFailure)
            return Result.Failure<Agenda>(validationResult.Error);

        var agenda = new Agenda { Id = Guid.CreateVersion7(), UserIdCreated = data.UserIdCreated };
        agenda.Apply(data);

        agenda.AddDomainEvent(new AgendaCreatedDomainEvent(agenda.Id));
        return Result.Success(agenda);
    }

    /// <summary>
    /// Updates this agenda after validating the provided data. The creator user is not changed.
    /// </summary>
    public Result Update(AgendaCreationData data)
    {
        var validationResult = Validate(data);
        if (validationResult.IsFailure)
            return validationResult;

        Apply(data);
        return Result.Success();
    }

    /// <summary>
    /// Applies a partial update: every member of <paramref name="data"/> that is set replaces the current value, the
    /// others are kept. The merged data is validated like <see cref="Update"/>. The creator user is not changed.
    /// </summary>
    /// <param name="data">The values to change.</param>
    public Result Patch(AgendaPatchData data)
    {
        return Update(new AgendaCreationData
        {
            Code = data.Code ?? Code,
            FullName = data.FullName ?? FullName,
            Email = data.Email ?? Email!.Value,
            Enabled = data.Enabled ?? Enabled,
            ImagePath = data.ImagePath ?? ImagePath,
            DirectLine = data.DirectLine ?? DirectLine,
            Internal = data.Internal ?? Internal,
            Corporate = data.Corporate ?? Corporate,
            DirectAccess = data.DirectAccess ?? DirectAccess,
            WorkstationId = data.WorkstationId ?? WorkstationId,
            LocationId = data.LocationId ?? LocationId,
            UserIdCreated = UserIdCreated
        });
    }

    /// <summary>
    /// Changes the relative path of the agenda photo. Other members are not changed.
    /// </summary>
    /// <param name="imagePath">The relative path of the stored photo (e.g. <c>agendas/{name}.png</c>).</param>
    public Result ChangeImagePath(string imagePath)
    {
        if (string.IsNullOrWhiteSpace(imagePath))
            return Result.Failure(AgendaErrors.ImagePathCannotBeNullOrWhitespace);

        ImagePath = imagePath.Trim();
        return Result.Success();
    }

    /// <summary>
    /// Removes the relative path of the agenda photo. Other members are not changed.
    /// </summary>
    public Result RemoveImagePath()
    {
        if (ImagePath is null)
            return Result.Failure(AgendaErrors.PhotoNotFound);

        ImagePath = null;
        return Result.Success();
    }

    private void Apply(AgendaCreationData data)
    {
        Code = data.Code;
        FullName = data.FullName;
        Email = Email.Create(data.Email).Value;
        Enabled = data.Enabled;
        ImagePath = NullIfBlank(data.ImagePath);
        DirectLine = NullIfBlank(data.DirectLine);
        Internal = NullIfBlank(data.Internal);
        Corporate = NullIfBlank(data.Corporate);
        DirectAccess = NullIfBlank(data.DirectAccess);
        WorkstationId = data.WorkstationId;
        LocationId = data.LocationId;
    }

    private static Result Validate(AgendaCreationData data)
    {
        if (string.IsNullOrWhiteSpace(data.Code))
            return Result.Failure(AgendaErrors.CodeCannotBeNullOrWhitespace);

        if (string.IsNullOrWhiteSpace(data.FullName))
            return Result.Failure(AgendaErrors.FullNameCannotBeNullOrWhitespace);

        var emailResult = Email.Create(data.Email);
        if (emailResult.IsFailure)
            return Result.Failure(emailResult.Error);

        if (data.WorkstationId == Guid.Empty)
            return Result.Failure(AgendaErrors.WorkstationIdCannotBeEmpty);

        if (data.LocationId == Guid.Empty)
            return Result.Failure(AgendaErrors.LocationIdCannotBeEmpty);

        if (data.UserIdCreated == Guid.Empty)
            return Result.Failure(AgendaErrors.UserIdCreatedCannotBeEmpty);

        return Result.Success();
    }

    private static string? NullIfBlank(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
