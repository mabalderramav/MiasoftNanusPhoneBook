using MiasoftNanus.PhoneBook.Domain.Abstractions;
using MiasoftNanus.PhoneBook.Domain.Agendas.Entities;

namespace MiasoftNanus.PhoneBook.Domain.Agendas.Repositories;

/// <summary>
/// Represents a repository interface that provides data access operations for the <see cref="Agenda"/> entity.
/// </summary>
/// <remarks>
/// This interface extends the generic repository contract, inheriting standard methods for CRUD operations.
/// Specific business logic or queries related to the <see cref="Agenda"/> entity can be defined here.
/// </remarks>
public interface IAgendaRepository : IGenericRepository<Agenda, Guid>;