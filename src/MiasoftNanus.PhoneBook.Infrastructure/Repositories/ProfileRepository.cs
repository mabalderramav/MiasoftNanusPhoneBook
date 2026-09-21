using MiasoftNanus.PhoneBook.Domain.Profiles.Entities;
using MiasoftNanus.PhoneBook.Domain.Profiles.Repositories;
using Microsoft.EntityFrameworkCore;

namespace MiasoftNanus.PhoneBook.Infrastructure.Repositories;

/// <summary>
/// Provides the implementation for managing Profile entities within the application.
/// This repository is responsible for querying and performing operations on Profile data
/// using the application's database context.
/// </summary>
internal sealed class ProfileRepository(AppDbContext appDbContext) : Repository<Profile>(appDbContext), IProfileRepository
{
    /// <summary>
    /// Retrieves a profile entity by its name.
    /// </summary>
    /// <param name="profileName">The name of the profile to retrieve.</param>
    /// <param name="cancellationToken">The cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains the profile entity if found;
    /// otherwise, null.
    /// </returns>
    public async Task<Profile?> GetByNameAsync(string profileName, CancellationToken cancellationToken = default)
    {
        return await AppDbContext.Set<Profile>().FirstOrDefaultAsync
            (e => e.ProfileName == profileName, cancellationToken);
    }
}