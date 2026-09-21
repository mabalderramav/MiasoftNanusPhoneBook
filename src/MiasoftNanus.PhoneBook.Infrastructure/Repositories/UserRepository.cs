using MiasoftNanus.PhoneBook.Domain.Users.Entities;
using MiasoftNanus.PhoneBook.Domain.Users.Repositories;

namespace MiasoftNanus.PhoneBook.Infrastructure.Repositories;

/// <summary>
/// Represents a repository implementation for performing data access operations related to user entities.
/// This repository provides an abstraction layer for managing user records in the database.
/// Inherits from the generic <see cref="Repository{T}"/>, where T is the <see cref="User"/> entity.
/// Implements the <see cref="IUserRepository"/> interface for user-specific data access methods.
/// </summary>
internal sealed class UserRepository(AppDbContext appDbContext) : Repository<User>(appDbContext), IUserRepository;