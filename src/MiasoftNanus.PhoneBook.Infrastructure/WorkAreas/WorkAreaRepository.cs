using MiasoftNanus.PhoneBook.Domain.WorkAreas.Entities;
using MiasoftNanus.PhoneBook.Domain.WorkAreas.Repositories;
using MiasoftNanus.PhoneBook.Infrastructure.Shared.Repositories;

namespace MiasoftNanus.PhoneBook.Infrastructure.WorkAreas;

/// <summary>
/// A repository implementation for managing <see cref="WorkArea"/> entities.
/// </summary>
/// <remarks>
/// This class provides an implementation of the <see cref="IWorkAreaRepository"/> interface
/// and extends the <see cref="CrudRepository{T}"/> base class to provide common
/// data management functionality for <see cref="WorkArea"/> domain entities.
/// </remarks>
/// <example>
/// This class is designed to be used by higher-level services that handle business logic
/// related to <see cref="WorkArea"/> representations.
/// </example>
public class WorkAreaRepository(AppDbContext appDbContext) :
    CrudRepository<WorkArea>(appDbContext), IWorkAreaRepository;