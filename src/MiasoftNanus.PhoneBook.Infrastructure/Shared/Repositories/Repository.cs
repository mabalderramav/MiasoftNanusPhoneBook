using MiasoftNanus.PhoneBook.Domain.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace MiasoftNanus.PhoneBook.Infrastructure.Shared.Repositories;

/// <summary>
/// A generic repository base class for managing entities in the database.
/// This class provides methods to perform CRUD operations on entities
/// of type <typeparamref name="T"/> in the database.
/// </summary>
/// <typeparam name="T">
/// The type of the entity that extends the <see cref="Entity"/> base class.
/// </typeparam>
internal abstract class Repository<T>(AppDbContext appDbContext)
    where T : Entity
{
    protected readonly AppDbContext AppDbContext = appDbContext;

    /// <summary>
    /// Asynchronously retrieves an entity of type <typeparamref name="T"/> from the database by its unique identifier.
    /// </summary>
    /// <typeparam name="T">
    /// The type of the entity that extends the <see cref="Entity"/> base class.
    /// </typeparam>
    /// <param name="id">
    /// The unique identifier of the entity to retrieve.
    /// </param>
    /// <param name="cancellationToken">
    /// A token to observe while waiting for the task to complete. The default value is <see cref="CancellationToken.None"/>.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains the entity of type
    /// <typeparamref name="T"/> if found; otherwise, <c>null</c>.
    /// </returns>
    public async Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await AppDbContext.Set<T>().FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    /// <summary>
    /// Adds a new entity of type <typeparamref name="T"/> to the database context.
    /// </summary>
    /// <typeparam name="T">
    /// The type of the entity that extends the <see cref="Entity"/> base class.
    /// </typeparam>
    /// <param name="entity">
    /// The entity of type <typeparamref name="T"/> to be added to the database context.
    /// </param>
    public void Add(T entity)
    {
        AppDbContext.Set<T>().Add(entity);
    }

    /// <summary>
    /// Updates the specified entity of type <typeparamref name="T"/> in the database.
    /// </summary>
    /// <typeparam name="T">
    /// The type of the entity that extends the <see cref="Entity"/> base class.
    /// </typeparam>
    /// <param name="entity">
    /// The entity of type <typeparamref name="T"/> to update in the database.
    /// </param>
    public void Update(T entity)
    {
        AppDbContext.Set<T>().Update(entity);
    }

    /// <summary>
    /// Removes the specified entity of type <typeparamref name="T"/> from the database context.
    /// </summary>
    /// <typeparam name="T">
    /// The type of the entity that extends the <see cref="Entity"/> base class.
    /// </typeparam>
    /// <param name="entity">
    /// The entity to be removed from the database context.
    /// </param>
    public void Remove(T entity)
    {
        AppDbContext.Set<T>().Remove(entity);
    }
}