using MiasoftNanus.PhoneBook.Domain.Abstractions;
using MiasoftNanus.PhoneBook.Domain.WorkAreas.Entities;

namespace MiasoftNanus.PhoneBook.Domain.WorkAreas.Repositories;

/// <summary>
/// Defines a repository interface for managing <see cref="WorkArea"/> entities within the application domain.
/// </summary>
/// <remarks>
/// The <c>IWorkAreaRepository</c> interface extends the functionality
/// of the generic <see cref="IGenericRepository{T,TId}"/>
/// interface, providing CRUD operations specifically for the <see cref="WorkArea"/> entity type. The primary identifier
/// for <see cref="WorkArea"/> entities is of type <see cref="Guid"/>.
/// 
/// This interface can be implemented to interact with various data sources such as databases, APIs,
/// or in-memory collections, depending on the application architecture and requirements.
/// </remarks>
public interface IWorkAreaRepository : IGenericRepository<WorkArea, Guid>;