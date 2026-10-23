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
public interface IAgendaRepository : IGenericRepository<Agenda, Guid>
{
    /// <summary>
    /// Retrieves an agenda by its identifier with its workstation, location, and creator user loaded.
    /// The result is not tracked.
    /// </summary>
    /// <param name="id">The unique identifier of the agenda to retrieve.</param>
    /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
    /// <returns>The agenda with its details, or <c>null</c> if it does not exist.</returns>
    Task<Agenda?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all agendas with their workstation, location, and creator user loaded. The result is not tracked.
    /// </summary>
    /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
    /// <returns>The agendas with their details.</returns>
    Task<ICollection<Agenda>> GetAllWithDetailsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Loads the workstation, location, and creator user of a tracked agenda, skipping the ones already loaded.
    /// </summary>
    /// <param name="agenda">The tracked agenda.</param>
    /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
    Task LoadDetailsAsync(Agenda agenda, CancellationToken cancellationToken = default);
}
