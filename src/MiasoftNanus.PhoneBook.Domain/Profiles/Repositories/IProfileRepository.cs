using MiasoftNanus.PhoneBook.Domain.Abstractions;
using MiasoftNanus.PhoneBook.Domain.Profiles.Entities;

namespace MiasoftNanus.PhoneBook.Domain.Profiles.Repositories;

/// <summary>
/// Represents a repository interface for managing <c>Profile</c> entities.
/// Provides methods for CRUD operations and specific profile-related queries.
/// </summary>
public interface IProfileRepository : IGenericRepository<Profile, Guid>
{
    /// <summary>
    /// Asynchronously retrieves a <c>Profile</c> entity by its unique name.
    /// </summary>
    /// <param name="profileName">The unique name of the profile to be retrieved.</param>
    /// <param name="cancellationToken">
    /// A <c>CancellationToken</c> to observe while waiting for the task to complete.
    /// Allows the operation to be canceled.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains
    /// the <c>Profile</c> entity if found, or <c>null</c> if no matching profile is located.
    /// </returns>
    Task<Profile?> GetByNameAsync(string profileName, CancellationToken cancellationToken = default);
}