using MiasoftNanus.PhoneBook.Domain.Users.Entities;
using MiasoftNanus.PhoneBook.Domain.Users.Repositories;
using MiasoftNanus.PhoneBook.Infrastructure.Shared.Repositories;

namespace MiasoftNanus.PhoneBook.Infrastructure.Users;

/// <summary>
/// Represents a repository for performing CRUD (Create, Read, Update, Delete) operations
/// on the User entity within the system.
/// </summary>
/// <remarks>
/// This class serves as the implementation of <see cref="IUserRepository"/> and inherits
/// from the <see cref="CrudRepository{T}"/> base class. It leverages the <see cref="AppDbContext"/>
/// for database interactions specific to the User entity. The class encapsulates data
/// access logic, ensuring a centralized approach to User entity persistence.
/// </remarks>
/// <threadsafety>
/// This class is not thread-safe. Accessing database operations concurrently should
/// ensure thread-safety mechanisms at higher layers in the application.
/// </threadsafety>
internal sealed class UserRepository(AppDbContext appDbContext) : 
    CrudRepository<User>(appDbContext), IUserRepository;