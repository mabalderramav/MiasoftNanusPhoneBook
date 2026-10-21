using MiasoftNanus.PhoneBook.Application.Abstractions.CQRS;
using MiasoftNanus.PhoneBook.Domain.Abstractions;
using MiasoftNanus.PhoneBook.Domain.Workstations.Entities;

namespace MiasoftNanus.PhoneBook.Application.Workstations.UseCases.Commands.UpdateWorkstation;

/// <summary>
/// Represents a command to update a workstation.
/// </summary>
public record UpdateWorkstationCommand(
    Guid Id,
    string Code,
    string Name,
    string? Description,
    Guid WorkAreaId
) : ICommand<Result<Workstation>>;
