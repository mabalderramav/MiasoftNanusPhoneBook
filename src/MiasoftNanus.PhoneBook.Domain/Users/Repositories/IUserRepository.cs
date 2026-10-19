using MiasoftNanus.PhoneBook.Domain.Abstractions;
using MiasoftNanus.PhoneBook.Domain.Users.Entities;

namespace MiasoftNanus.PhoneBook.Domain.Users.Repositories;

/// <summary>
/// Represents the repository interface for managing User entities.
/// Provides operations for adding users and retrieving users by their unique identifiers.
/// Inherits from IGenericRepository for generic CRUD operations.
/// </summary>
public interface IUserRepository : IGenericRepository<User, Guid>;