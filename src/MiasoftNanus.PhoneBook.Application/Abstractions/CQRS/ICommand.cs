namespace MiasoftNanus.PhoneBook.Application.Abstractions.CQRS
{
    /// <summary>
    /// Represents a command in the CQRS architecture that is used to encapsulate the data needed
    /// for a specific operation and is sent to an appropriate command handler for processing.
    /// </summary>
    /// <typeparam name="TResult">The type of the result produced by executing the command.</typeparam>
#pragma warning disable S2326 // TResult associates each command with the result produced by its handler.
    public interface ICommand<TResult>;
#pragma warning restore S2326

    /// <summary>
    /// Defines a handler for processing a specific command in the CQRS architecture.
    /// A command handler encapsulates the business logic required to process a command
    /// and produce the corresponding result.
    /// </summary>  
    /// <typeparam name="TCommand">The type of the command to be handled. Must implement
    /// <see cref="ICommand{TResult}"/>.</typeparam>
    /// <typeparam name="TResult">The type of the result produced by the command handler.</typeparam>
    public interface ICommandHandler<in TCommand, TResult>
        where TCommand : ICommand<TResult>
    {
        /// <summary>
        /// Asynchronously handles the specified command and produces a result.
        /// </summary>
        /// <typeparam name="TCommand">The type of the command to handle.
        /// Must implement <see cref="ICommand{TResult}"/>.</typeparam>
        /// <typeparam name="TResult">The type of the result produced by processing the command.</typeparam>
        /// <param name="command">The command to be processed.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
        /// <returns>A task representing the operation, containing the result of processing the command.</returns>
        Task<TResult> HandleAsync(TCommand command, CancellationToken cancellationToken = default);
    }
}
