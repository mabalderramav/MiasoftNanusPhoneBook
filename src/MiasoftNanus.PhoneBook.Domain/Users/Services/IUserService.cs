using MiasoftNanus.PhoneBook.Domain.Abstractions.Services;
using MiasoftNanus.PhoneBook.Domain.Users.Entities;

namespace MiasoftNanus.PhoneBook.Domain.Users.Services;

/// <summary>
/// Provides a contract for managing user entities.
/// </summary>
public interface IUserService : ICrudService<User, Guid>;
