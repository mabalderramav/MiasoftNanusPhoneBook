using MediatR;
using MiasoftNanus.PhoneBook.Domain.Abstractions;

namespace MiasoftNanus.PhoneBook.Application.Abstractions.Messaging;

/// <summary>
/// Defines a contract for handling commands within the application.
/// </summary>
/// <typeparam name="TCommand">
/// The type of the command being handled. Must implement the <see cref="ICommand"/> interface.
/// </typeparam>
public interface ICommandHandler<in TCommand> : IRequestHandler<TCommand, Result>
    where TCommand : ICommand
{
}

/// <summary>
/// Represents a contract for handling commands that produce a result within the application.
/// </summary>
/// <typeparam name="TCommand">
/// The type of the command being handled. Must implement the <see cref="ICommand"/> interface.
/// </typeparam>
/// <typeparam name="TResponse">
/// The type of the response produced by the command.
/// </typeparam>
public interface ICommandHandler<in TCommand, TResponse> : IRequestHandler<TCommand, Result<TResponse>>
    where TCommand : ICommand<TResponse>
{
}