using MiasoftNanus.PhoneBook.Domain.Abstractions;
using MiasoftNanus.PhoneBook.Domain.Abstractions.Services;
using MiasoftNanus.PhoneBook.Domain.Users.Entities;

namespace MiasoftNanus.PhoneBook.Domain.Users.Services;

/// <summary>
/// Provides a contract for managing user entities.
/// </summary>
public interface IUserService : ICrudService<User, Guid>
{
    /// <summary>
    /// Asynchronously updates the details of an existing user. The password is not changed.
    /// </summary>
    /// <param name="id">The unique identifier of the user to be updated.</param>
    /// <param name="data">The new user details.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A result containing the updated user, or an error if the operation failed.</returns>
    Task<Result<User>> UpdateDetailsAsync(Guid id, UserData data, CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously replaces the password of an existing user.
    /// </summary>
    /// <param name="id">The unique identifier of the user.</param>
    /// <param name="password">The new raw password.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A result indicating the success or failure of the operation.</returns>
    Task<Result> ChangePasswordAsync(Guid id, string password, CancellationToken cancellationToken = default);
}
