using MiasoftNanus.PhoneBook.Application.Abstractions.CQRS;
using MiasoftNanus.PhoneBook.Domain.Abstractions;
using MiasoftNanus.PhoneBook.Domain.Workstations.Entities;

namespace MiasoftNanus.PhoneBook.Application.Workstations.UseCases.Commands.CreateWorkstation;

/// <summary>
/// Represents a command to create a workstation.
/// </summary>
public record CreateWorkstationCommand(
    string Code,
    string Name,
    string? Description,
    Guid WorkAreaId
) : ICommand<Result<Workstation>>;
