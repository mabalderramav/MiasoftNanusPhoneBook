using MiasoftNanus.PhoneBook.Application.Abstractions.CQRS;
using MiasoftNanus.PhoneBook.Domain.Abstractions;

namespace MiasoftNanus.PhoneBook.Application.WorkAreas.UseCases.Commands.DeleteWorkArea;

/// <summary>
/// Represents a command to delete a work area.
/// </summary>
public record DeleteWorkAreaCommand(Guid Id) : ICommand<Result>;
