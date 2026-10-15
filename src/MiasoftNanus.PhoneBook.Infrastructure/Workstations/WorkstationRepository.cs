using MiasoftNanus.PhoneBook.Domain.Workstations.Entities;
using MiasoftNanus.PhoneBook.Domain.Workstations.Repositories;
using MiasoftNanus.PhoneBook.Infrastructure.Shared.Repositories;

namespace MiasoftNanus.PhoneBook.Infrastructure.Workstations;

/// <summary>
/// Represents a repository for managing workstations within the domain.
/// Provides basic CRUD operations and ensures integration with the application's database context.
/// </summary>
/// <remarks>
/// Inherits from <see cref="CrudRepository{T}"/>, providing generic repository functionality for the
/// <see cref="Workstation"/> entity.
/// Implements the <see cref="IWorkstationRepository"/> interface to adhere to repository patterns
/// specific to workstations.
/// </remarks>
/// <example>
/// This class interacts with the workstation domain entity and leverages the application's persistence layer
/// to perform operations
/// such as creating, reading, updating, and deleting workstation data.
/// </example>
public class WorkstationRepository(AppDbContext appDbContext) :
    CrudRepository<Workstation>(appDbContext), IWorkstationRepository;