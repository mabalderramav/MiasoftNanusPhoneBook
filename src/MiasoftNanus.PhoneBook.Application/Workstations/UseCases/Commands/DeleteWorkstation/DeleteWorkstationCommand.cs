using MiasoftNanus.PhoneBook.Application.Abstractions.CQRS;
using MiasoftNanus.PhoneBook.Domain.Abstractions;

namespace MiasoftNanus.PhoneBook.Application.Workstations.UseCases.Commands.DeleteWorkstation;

/// <summary>
/// Represents a command to delete a workstation.
/// </summary>
public record DeleteWorkstationCommand(Guid Id) : ICommand<Result>;
