using MiasoftNanus.PhoneBook.Application.Abstractions.CQRS;
using MiasoftNanus.PhoneBook.Domain.Abstractions;
using MiasoftNanus.PhoneBook.Domain.WorkAreas.Entities;

namespace MiasoftNanus.PhoneBook.Application.WorkAreas.UseCases.Commands.CreateWorkArea;

/// <summary>
/// Represents a command to create a work area.
/// </summary>
public record CreateWorkAreaCommand(
    string Code,
    string Name,
    string? Description
) : ICommand<Result<WorkArea>>;
