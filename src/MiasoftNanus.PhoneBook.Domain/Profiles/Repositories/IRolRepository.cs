using MiasoftNanus.PhoneBook.Domain.Profiles.Entities;

namespace MiasoftNanus.PhoneBook.Domain.Profiles.Repositories;

/// <summary>
/// Defines the contract for repository operations related to the <c>Profile</c> entity in the domain.
/// </summary>
/// <remarks>
/// The <c>IRolRepository</c> interface provides methods to interact with and manage
/// <c>Profile</c> entities, specifically for retrieving profiles based on their unique name.
/// This repository abstraction supports asynchronous operations and leverages cancellation tokens
/// to allow cooperative cancellation of tasks.
/// </remarks>
public interface IRolRepository
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