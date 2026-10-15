using MiasoftNanus.PhoneBook.Domain.Abstractions;
using MiasoftNanus.PhoneBook.Domain.Workstations.Entities;

namespace MiasoftNanus.PhoneBook.Domain.Workstations.Repositories;

/// <summary>
/// Defines the repository interface for managing <see cref="Workstation"/> entities.
/// </summary>
/// <remarks>
/// Provides data access operations specific to the <see cref="Workstation"/> entity, while inheriting
/// standard CRUD operations from <see cref="IGenericRepository{T, TId}"/>. This interface is tailored
/// to the domain's need for managing workstation-related data.
/// </remarks>
/// <seealso cref="Workstation"/>
/// <seealso cref="IGenericRepository{T, TId}"/>
public interface IWorkstationRepository : IGenericRepository<Workstation, Guid>;