using MiasoftNanus.PhoneBook.Application.Abstractions.CQRS;
using MiasoftNanus.PhoneBook.Domain.Abstractions;
using MiasoftNanus.PhoneBook.Domain.WorkAreas.Entities;

namespace MiasoftNanus.PhoneBook.Application.WorkAreas.UseCases.Commands.UpdateWorkArea;

/// <summary>
/// Represents a command to update a work area.
/// </summary>
public record UpdateWorkAreaCommand(
    Guid Id,
    string Code,
    string Name,
    string? Description
) : ICommand<Result<WorkArea>>;
