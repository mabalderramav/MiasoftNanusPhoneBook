using MiasoftNanus.PhoneBook.Domain.Abstractions;
using MiasoftNanus.PhoneBook.Domain.WorkAreas.Entities;
using MiasoftNanus.PhoneBook.Domain.Workstations.Errors;

namespace MiasoftNanus.PhoneBook.Domain.Workstations.Entities;

/// <summary>
/// Represents a workstation entity within the domain.
/// </summary>
/// <remarks>
/// A workstation typically contains details such as a unique code, a name,
/// and an optional description. This entity is part of the domain model
/// and serves to represent the concept of a workstation within the context
/// of the system.
/// </remarks>
public class Workstation : Entity
{
    private Workstation()
    {
        Code = string.Empty;
        Name = string.Empty;
        Description = null;
        WorkAreaId = Guid.Empty;
        WorkArea = null;
    }

    public required string Code { get; init; }
    public required string Name { get; init; }
    public string? Description { get; init; }
    public Guid? WorkAreaId { get; init; }
    public WorkArea? WorkArea { get; init; }

    /// <summary>
    /// Creates a new instance of the <see cref="Workstation"/> class.
    /// </summary>
    /// <param name="code">The unique code identifying the workstation. Must not be null or whitespace.</param>
    /// <param name="name">The name of the workstation. Must not be null or whitespace.</param>
    /// <param name="description">A brief description of the workstation. Can be null.</param>
    /// <returns>
    /// A <see cref="Result{Workstation}"/> object indicating the success or failure of the creation process.
    /// If successful, the result contains the created <see cref="Workstation"/> instance.
    /// </returns>
    public static Result<Workstation> Create(string code, string name, string? description)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return Result.Failure<Workstation>(WorkstationErrors.CodeCannotBeNullOrWhitespace);
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            return Result.Failure<Workstation>(WorkstationErrors.NameCannotBeNullOrWhitespace);
        }

        return new Workstation()
        {
            Id = Guid.NewGuid(),
            Code = code,
            Name = name,
            Description = description
        };
    }
}