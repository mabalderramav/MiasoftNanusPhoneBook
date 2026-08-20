using MiasoftNanus.PhoneBook.Domain.Users.Entities;

namespace MiasoftNanus.PhoneBook.Domain.Users.Repositories;

/// <summary>
/// Defines a contract for a repository managing User entities within the phonebook domain.
/// </summary>
/// <remarks>
/// The IUserRepository interface provides methods to interact with the underlying data store
/// for User entities, enabling operations such as adding and retrieving user information.
/// Implementations of this interface should handle the persistence and retrieval logic,
/// adhering to the domain requirements of the phonebook system.
/// </remarks>
public interface IUserRepository
{
    /// <summary>
    /// Adds a new user to the repository.
    /// </summary>
    /// <param name="user">The user entity to be added to the repository.</param>
    void Add(User user);

    /// <summary>
    /// Retrieves a User entity by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the User entity to retrieve.</param>
    /// <param name="cancellationToken">
    /// A token to monitor for cancellation requests. Allows the operation to be canceled if needed.
    /// </param>
    /// <returns>
    /// A task representing the asynchronous operation. The task result contains the User entity
    /// if found; otherwise, null.
    /// </returns>
    Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
}