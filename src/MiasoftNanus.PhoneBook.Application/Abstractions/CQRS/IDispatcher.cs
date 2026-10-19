namespace MiasoftNanus.PhoneBook.Application.Abstractions.CQRS;

/// <summary>
/// Defines the contract for a dispatcher that delegates the execution of commands and queries
/// within the CQRS architecture.
/// </summary>
public interface IDispatcher
{
    /// <summary>
    /// Sends a command to the appropriate command handler for processing asynchronously.
    /// </summary>
    /// <typeparam name="TResult">The type of the result returned by the command handler.</typeparam>
    /// <param name="command">The command to be executed.</param>
    /// <param name="cancellationToken">
    /// A token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.
    /// </param>
    /// <returns>A task that represents the asynchronous operation, with the result being the output
    /// of the command handler.</returns>
    Task<TResult> SendCommandAsync<TResult>(
        ICommand<TResult> command, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a query to the appropriate query handler for processing asynchronously.
    /// </summary>
    /// <typeparam name="TResult">The type of the result returned by the query handler.</typeparam>
    /// <param name="query">The query to be executed.</param>
    /// <param name="cancellationToken">
    /// A token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.
    /// </param>
    /// <returns>A task that represents the asynchronous operation, with the result being the output
    /// of the query handler.</returns>
    Task<TResult> SendQueryAsync<TResult>(IQuery<TResult> query, CancellationToken cancellationToken = default);
}