using MiasoftNanus.PhoneBook.Application.Abstractions.CQRS;
using MiasoftNanus.PhoneBook.Domain.Abstractions;
using MiasoftNanus.PhoneBook.Domain.Abstractions.Services;

namespace MiasoftNanus.PhoneBook.Application.Abstractions.Services;

/// <summary>
/// Represents a base class for implementing CRUD services using the CQRS design pattern.
/// This service provides asynchronous methods for creating, retrieving, updating, and deleting entities
/// while using commands and queries for operations.
/// </summary>
/// <typeparam name="T">The type of the entity the service operates on. Must inherit from
/// <see cref="Entity"/>.</typeparam>
/// <typeparam name="TId">The type of the identifier used for the entity.</typeparam>
public abstract class CrudCqrsService<T, TId>(IDispatcher dispatcher) : ICrudService<T, TId>
    where T : Entity
{
    protected IDispatcher Dispatcher { get; } = dispatcher;

    /// <summary>
    /// Builds the command that creates a new entity of type <paramref name="entity"/>.
    /// </summary>
    /// <param name="entity">The entity instance to be created.</param>
    /// <returns>An implementation of <see cref="ICommand{T}"/> used to execute the creation operation.</returns>
    protected abstract ICommand<Result<T>> BuildCreateCommand(T entity);

    /// <summary>
    /// Builds the query that retrieves an entity of type <typeparamref name="T"/> by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the entity to be retrieved.</param>
    /// <returns>An implementation of <see cref="IQuery{T}"/> used to execute the retrieval operation.</returns>
    protected abstract IQuery<Result<T>> BuildGetByIdQuery(TId id);

    /// <summary>
    /// Builds the query that retrieves all entities of type <typeparamref name="T"/>.
    /// </summary>
    /// <returns>An implementation of <see cref="IQuery{T}"/> used to execute the retrieval operation for a collection
    /// of entities.</returns>
    protected abstract IQuery<Result<ICollection<T>>> BuildGetAllQuery();

    /// <summary>
    /// Builds the command that updates an existing entity of type <paramref name="entity"/>.
    /// </summary>
    /// <param name="id">The unique identifier of the entity to be updated.</param>
    /// <param name="entity">The entity instance to be updated.</param>
    /// <returns>An implementation of <see cref="ICommand{T}"/> used to execute the update operation.</returns>
    protected abstract ICommand<Result<T>> BuildUpdateCommand(TId id, T entity);

    /// <summary>
    /// Builds the command that deletes the specified entity of type <paramref>
    ///     <name>entity</name>
    /// </paramref>
    /// .
    /// </summary>
    /// <param name="id">The unique identifier of the entity to be deleted.</param>
    /// <returns>An implementation of <see cref="ICommand{TResult}"/> used to execute the delete operation,
    /// which returns a <see cref="Result"/> indicating the success or failure of the operation.</returns>
    protected abstract ICommand<Result> BuildDeleteCommand(TId id);

    public virtual Task<Result<T>> CreateAsync(T entity, CancellationToken cancellationToken = default) =>
        Dispatcher.SendCommandAsync(BuildCreateCommand(entity), cancellationToken);

    public virtual Task<Result<T>> GetByIdAsync(TId id, CancellationToken cancellationToken = default) =>
        Dispatcher.SendQueryAsync(BuildGetByIdQuery(id), cancellationToken);

    public virtual Task<Result<ICollection<T>>> GetAllAsync(CancellationToken cancellationToken = default) =>
        Dispatcher.SendQueryAsync(BuildGetAllQuery(), cancellationToken);

    public virtual Task<Result<T>> UpdateAsync(TId id, T entity, CancellationToken cancellationToken = default) =>
        Dispatcher.SendCommandAsync(BuildUpdateCommand(id, entity), cancellationToken);

    public virtual Task<Result> DeleteAsync(TId id, CancellationToken cancellationToken = default) =>
        Dispatcher.SendCommandAsync(BuildDeleteCommand(id), cancellationToken);
}