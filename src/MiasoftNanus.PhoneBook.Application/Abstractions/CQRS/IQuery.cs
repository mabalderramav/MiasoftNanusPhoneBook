namespace MiasoftNanus.PhoneBook.Application.Abstractions.CQRS;

/// <summary>
/// Represents a contract for a query in the CQRS architecture. This interface defines a marker
/// for queries that are intended to retrieve data and are processed by query handlers.
/// </summary>
/// <typeparam name="TResult">
/// The type of the result produced by executing the query.
/// </typeparam>
#pragma warning disable S2326 // TResult associates each query with the result produced by its handler.
public interface IQuery<TResult>;
#pragma warning restore S2326

/// <summary>
/// Defines a contract for handling queries in the CQRS architecture. Implementors of this
/// interface are responsible for processing specific types of queries and producing the
/// corresponding result.
/// </summary>
/// <typeparam name="TQuery">
/// The type of the query to be handled. This must implement the <see cref="IQuery{TResult}"/> interface.
/// </typeparam>
/// <typeparam name="TResult">
/// The type of the result produced by handling the query.
/// </typeparam>
public interface IQueryHandler<in TQuery, TResult>
    where TQuery : IQuery<TResult>
{
    /// <summary>
    /// Processes the given query asynchronously and produces a result of the specified type.
    /// This method is implemented by query handlers to handle a specific type of query in the CQRS architecture.
    /// </summary>
    /// <typeparam name="TQuery">
    /// The type of the query being handled. Must implement the <see cref="IQuery{TResult}"/> interface.
    /// </typeparam>
    /// <typeparam name="TResult">
    /// The type of the result produced by executing the query.
    /// </typeparam>
    /// <param name="query">
    /// The instance of the query to be executed. Cannot be null.
    /// </param>
    /// <param name="cancellationToken">
    /// A token to observe while waiting for the task to complete. The default value is
    /// <see cref="CancellationToken.None"/>.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task's result is the result of processing the query.
    /// </returns>
    Task<TResult> HandleAsync(TQuery query, CancellationToken cancellationToken = default);
}
