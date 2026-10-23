using MiasoftNanus.PhoneBook.Application.Abstractions.CQRS;
using MiasoftNanus.PhoneBook.Domain.Abstractions;
using MiasoftNanus.PhoneBook.Domain.Users.Entities;

namespace MiasoftNanus.PhoneBook.Application.Users.UseCases.Commands.PatchUser;

/// <summary>
/// Represents a command to partially update the details of a user. The password is changed by
/// <see cref="ChangeUserPassword.ChangeUserPasswordCommand"/>.
/// </summary>
public sealed record PatchUserCommand(Guid Id, UserPatchData Data) : ICommand<Result<User>>;
