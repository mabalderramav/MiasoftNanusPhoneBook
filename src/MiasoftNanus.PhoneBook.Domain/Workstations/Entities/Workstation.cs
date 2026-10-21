using MiasoftNanus.PhoneBook.Domain.Abstractions;
using MiasoftNanus.PhoneBook.Domain.Agendas.Entities;
using MiasoftNanus.PhoneBook.Domain.WorkAreas.Entities;
using MiasoftNanus.PhoneBook.Domain.Workstations.Errors;

namespace MiasoftNanus.PhoneBook.Domain.Workstations.Entities;

/// <summary>
/// Represents a workstation entity within the domain.
/// </summary>
/// <remarks>
/// A workstation typically contains details such as a unique code, a name,
/// an optional description and the work area it belongs to. This entity is part of the domain model
/// and serves to represent the concept of a workstation within the context
/// of the system.
/// </remarks>
public class Workstation : Entity
{
    private Workstation(Guid id, string code, string name, string? description, Guid workAreaId)
    {
        Id = id;
        Code = code;
        Name = name;
        Description = description;
        WorkAreaId = workAreaId;
    }

    public string Code { get; private set; }
    public string Name { get; private set; }
    public string? Description { get; private set; }
    public Guid WorkAreaId { get; private set; }
    public WorkArea? WorkArea { get; init; }
    public ICollection<Agenda>? Agendas { get; }

    /// <summary>
    /// Creates a new instance of the <see cref="Workstation"/> class.
    /// </summary>
    /// <param name="code">The unique code identifying the workstation. Must not be null or whitespace.</param>
    /// <param name="name">The name of the workstation. Must not be null or whitespace.</param>
    /// <param name="description">A brief description of the workstation. Can be null.</param>
    /// <param name="workAreaId">The identifier of the work area the workstation belongs to. Must not be empty.</param>
    /// <returns>
    /// A <see cref="Result{Workstation}"/> object indicating the success or failure of the creation process.
    /// If successful, the result contains the created <see cref="Workstation"/> instance.
    /// </returns>
    public static Result<Workstation> Create(string code, string name, string? description, Guid workAreaId)
    {
        var validationResult = Validate(code, name, workAreaId);
        return validationResult.IsFailure
            ? Result.Failure<Workstation>(validationResult.Error)
            : Result.Success(new Workstation(Guid.CreateVersion7(), code, name, description, workAreaId));
    }

    /// <summary>
    /// Updates this workstation after validating its required fields.
    /// </summary>
    public Result Update(string code, string name, string? description, Guid workAreaId)
    {
        var validationResult = Validate(code, name, workAreaId);
        if (validationResult.IsFailure)
            return validationResult;

        Code = code;
        Name = name;
        Description = description;
        WorkAreaId = workAreaId;
        return Result.Success();
    }

    private static Result Validate(string code, string name, Guid workAreaId)
    {
        if (string.IsNullOrWhiteSpace(code))
            return Result.Failure(WorkstationErrors.CodeCannotBeNullOrWhitespace);

        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure(WorkstationErrors.NameCannotBeNullOrWhitespace);

        if (workAreaId == Guid.Empty)
            return Result.Failure(WorkstationErrors.WorkAreaIdCannotBeEmpty);

        return Result.Success();
    }
}
