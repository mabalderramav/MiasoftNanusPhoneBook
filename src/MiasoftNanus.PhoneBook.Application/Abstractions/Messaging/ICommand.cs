using MediatR;
using MiasoftNanus.PhoneBook.Domain.Abstractions;

namespace MiasoftNanus.PhoneBook.Application.Abstractions.Messaging;

/// <summary>
/// Defines a contract for commands within the application.
/// </summary>
/// <remarks>
/// A command represents an operation or action that changes the state of the system,
/// adhering to the CQRS pattern. Implementations of this interface should encapsulate all
/// necessary data required to perform the operation.
/// </remarks>
public interface ICommand : IRequest<Result>, IBaseCommand
{
}

/// <summary>
/// Represents a contract for defining commands within the application's behavior.
/// </summary>
/// <remarks>
/// Commands are used to encapsulate all data required to perform a specific action or operation.
/// They adhere to the principles of the CQRS design pattern and are intended to invoke state changes
/// within the system while being handled by a corresponding handler.
/// </remarks>
public interface ICommand<TResponse> : IRequest<Result<TResponse>>, IBaseCommand
{
}

/// <summary>
/// Serves as a marker interface for commands within the application.
/// </summary>
/// <remarks>
/// This interface is used as a base contract to standardize the behavior of commands in the system.
/// It ensures consistency and helps enforce the principles of the CQRS pattern by grouping command-based
/// functionality across the application.
/// </remarks>
public interface IBaseCommand
{
}