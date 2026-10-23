using MiasoftNanus.PhoneBook.Application.Abstractions.CQRS;
using MiasoftNanus.PhoneBook.Domain.Abstractions;

namespace MiasoftNanus.PhoneBook.Application.Users.UseCases.Commands.ChangeUserPassword;

/// <summary>
/// Represents a command to change the password of a user.
/// </summary>
public sealed record ChangeUserPasswordCommand(Guid Id, string Password) : ICommand<Result>
{
    // Masks the raw password so it never leaks through logs.
    public override string ToString() => $"{nameof(ChangeUserPasswordCommand)} {{ Id = {Id}, {nameof(Password)} = ***** }}";
}
