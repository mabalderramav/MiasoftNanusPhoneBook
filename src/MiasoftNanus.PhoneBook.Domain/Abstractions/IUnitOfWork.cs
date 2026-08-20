namespace MiasoftNanus.PhoneBook.Domain.Abstractions;

/// <summary>
/// Represents a contract for a unit of work to manage and coordinate changes
/// across multiple repositories or operations within a single transactional scope.
/// </summary>
public interface IUnitOfWork
{
    /// <summary>
    /// Saves all changes made in the current unit of work asynchronously.
    /// This method ensures that all modifications across repositories are
    /// persisted within a single transactional scope.
    /// </summary>
    /// <param name="cancellationToken">
    /// A token to monitor for cancellation requests, allowing the operation
    /// to be canceled before completion.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous save operation. The task result contains
    /// the number of state entries written to the underlying database.
    /// </returns>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}