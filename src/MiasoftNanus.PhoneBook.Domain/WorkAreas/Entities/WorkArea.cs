using MiasoftNanus.PhoneBook.Domain.Abstractions;
using MiasoftNanus.PhoneBook.Domain.WorkAreas.Errors;
using MiasoftNanus.PhoneBook.Domain.Workstations.Entities;

namespace MiasoftNanus.PhoneBook.Domain.WorkAreas.Entities;

/// <summary>
/// Represents a work area entity within the domain.
/// </summary>
/// <remarks>
/// The <c>WorkArea</c> class encapsulates details related to a specific area of work or domain context.
/// It includes unique identification, a code, a name, and an optional description. This entity
/// inherits from the <c>Entity</c> base class, which provides unique identification and domain event management.
///
/// This class also provides a factory method <see cref="Create"/> to ensure proper initialization
/// and validation during the creation of a work area instance.
/// </remarks>
public class WorkArea : Entity
{
    private WorkArea(Guid id, string code, string name, string? description)
    {
        Id = id;
        Code = code;
        Name = name;
        Description = description;
    }

    public string Code { get; private set; }
    public string Name { get; private set; }
    public string? Description { get; private set; }
    public ICollection<Workstation>? Workstations { get; init; }

    /// <summary>
    /// Creates a new instance of the <see cref="WorkArea"/> class with the specified attributes
    /// and performs validation for required fields.
    /// </summary>
    /// <param name="code">The unique code representing the work area. Cannot be null, empty, or whitespace.</param>
    /// <param name="name">The name of the work area. Cannot be null, empty, or whitespace.</param>
    /// <param name="description">An optional description providing additional details about the work area.
    /// Can be null.</param>
    /// <returns>
    /// A <see cref="Result{WorkArea}"/> that either contains a successfully created <see cref="WorkArea"/> instance
    /// or failure information if validation fails.
    /// </returns>
    public static Result<WorkArea> Create(string code, string name, string? description)
    {
        var validationResult = Validate(code, name);
        return validationResult.IsFailure
            ? Result.Failure<WorkArea>(validationResult.Error)
            : Result.Success(new WorkArea(Guid.CreateVersion7(), code, name, description));
    }

    /// <summary>
    /// Updates this work area after validating its required fields.
    /// </summary>
    public Result Update(string code, string name, string? description)
    {
        var validationResult = Validate(code, name);
        if (validationResult.IsFailure)
            return validationResult;

        Code = code;
        Name = name;
        Description = description;
        return Result.Success();
    }

    private static Result Validate(string code, string name)
    {
        if (string.IsNullOrWhiteSpace(code))
            return Result.Failure(WorkAreaErrors.CodeCannotBeNullOrWhitespace);

        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure(WorkAreaErrors.NameCannotBeNullOrWhitespace);

        return Result.Success();
    }
}
