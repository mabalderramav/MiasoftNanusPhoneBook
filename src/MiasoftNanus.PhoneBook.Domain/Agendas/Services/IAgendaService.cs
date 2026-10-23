using MiasoftNanus.PhoneBook.Domain.Abstractions;
using MiasoftNanus.PhoneBook.Domain.Abstractions.Services;
using MiasoftNanus.PhoneBook.Domain.Agendas.Entities;

namespace MiasoftNanus.PhoneBook.Domain.Agendas.Services;

/// <summary>
/// Provides a contract for managing agenda entities.
/// </summary>
public interface IAgendaService : ICrudService<Agenda, Guid>
{
    /// <summary>
    /// Asynchronously applies a partial update to an existing agenda.
    /// </summary>
    /// <param name="id">The unique identifier of the agenda to be updated.</param>
    /// <param name="data">The values to change; unset members keep their current value.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A result containing the updated agenda, or an error if the operation failed.</returns>
    Task<Result<Agenda>> PatchAsync(Guid id, AgendaPatchData data, CancellationToken cancellationToken = default);
}
