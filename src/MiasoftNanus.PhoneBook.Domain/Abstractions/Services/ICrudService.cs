namespace MiasoftNanus.PhoneBook.Domain.Abstractions.Services;

/// <summary>
/// Defines a generic CRUD service interface for entities, providing
/// methods to create, read, update, and delete operations.
/// </summary>
/// <typeparam name="T">The type of the entity.</typeparam>
/// <typeparam name="TId">The type of the unique identifier for the entity.</typeparam>
public interface ICrudService<T , in TId> where T : Entity
{
    /// <summary>
    /// Asynchronously creates a new entity in the system.
    /// </summary>
    /// <param name="entity">The entity to be created.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A result containing the created entity, or an error if the operation failed.</returns>
    Task<Result<T>> CreateAsync(T entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously retrieves an entity by its unique identifier from the system.
    /// </summary>
    /// <param name="id">The unique identifier of the entity to be retrieved.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A result containing the entity matching the specified identifier, or an error if not found.</returns>
    Task<Result<T>> GetByIdAsync(TId id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously retrieves all entities from the system.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A result containing a collection of all entities.</returns>
    Task<Result<ICollection<T>>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously updates an existing entity in the system.
    /// </summary>
    /// <param name="entity">The entity with updated information.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A result containing the updated entity, or an error if the operation failed.</returns>
    Task<Result<T>> UpdateAsync(T entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously deletes an entity from the system by its unique identifier.
    /// </summary>
    /// <param name="entity">The entity to be deleted.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    Task<Result> DeleteAsync(T entity, CancellationToken cancellationToken = default);
}