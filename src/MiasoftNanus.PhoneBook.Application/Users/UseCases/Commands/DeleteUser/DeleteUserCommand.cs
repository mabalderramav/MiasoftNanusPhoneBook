using MiasoftNanus.PhoneBook.Application.Abstractions.CQRS;
using MiasoftNanus.PhoneBook.Domain.Abstractions;

namespace MiasoftNanus.PhoneBook.Application.Users.UseCases.Commands.DeleteUser;

/// <summary>
/// Represents a command to delete a user.
/// </summary>
public record DeleteUserCommand(Guid Id) : ICommand<Result>;
