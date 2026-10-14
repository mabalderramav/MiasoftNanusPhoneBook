namespace MiasoftNanus.PhoneBook.Domain.Abstractions;

/// <summary>
/// Defines the contract for a generic repository that provides standard data access operations for a given entity type.
/// </summary>
/// <typeparam name="T">The type of entity that the repository manages.</typeparam>
/// <typeparam name="TId">The type of the unique identifier for the entity.</typeparam>
public interface IGenericRepository<T, in TId>
{
    /// <summary>
    /// Retrieves an entity by its unique identifier from the data source.
    /// </summary>
    /// <param name="id">The unique identifier of the entity to retrieve.</param>
    /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous operation.
    /// The task result contains the entity retrieved from the data source.</returns>
    Task<T?> GetByIdAsync(TId id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all entities from the data source.
    /// </summary>
    /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous operation.
    /// The task result contains a collection of all entities retrieved from the data source.</returns>
    Task<ICollection<T>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new entity to the repository.
    /// </summary>
    /// <param name="entity">The entity to be added to the repository.</param>
    void Add(T entity);

    /// <summary>
    /// Updates an existing entity in the repository with new values.
    /// </summary>
    /// <param name="entity">The entity containing updated values to save in the repository.</param>
    void Update(T entity);

    /// <summary>
    /// Removes the entity with the specified unique identifier from the repository.
    /// </summary>
    /// <param name="entity">The entity to be removed from the repository.</param>
    void Remove(T entity);
}