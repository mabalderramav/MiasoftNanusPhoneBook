using MiasoftNanus.PhoneBook.Domain.Agendas.Entities;
using MiasoftNanus.PhoneBook.Domain.Agendas.Repositories;
using MiasoftNanus.PhoneBook.Infrastructure.Shared.Repositories;
using Microsoft.EntityFrameworkCore;

namespace MiasoftNanus.PhoneBook.Infrastructure.Agendas;

/// <summary>
/// Represents a repository responsible for managing operations related to the Agenda entity,
/// such as creating, reading, updating, and deleting agenda records in the database.
/// </summary>
/// <remarks>
/// This class extends the functionality of the <see cref="CrudRepository{T}"/> base class,
/// providing a concrete implementation specifically for the <see cref="Agenda"/> entity, and
/// implements the <see cref="IAgendaRepository"/> interface to ensure adherence to the
/// domain-specific repository contract.
/// </remarks>
/// <param name="appDbContext">
/// The instance of <see cref="AppDbContext"/> utilized by the repository to interact with the database.
/// </param>
public class AgendaRepository(AppDbContext appDbContext) : CrudRepository<Agenda>(appDbContext), IAgendaRepository
{
    public async Task<Agenda?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await WithDetails().FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
    }

    public async Task<ICollection<Agenda>> GetAllWithDetailsAsync(CancellationToken cancellationToken = default)
    {
        return await WithDetails().ToListAsync(cancellationToken);
    }

    public async Task LoadDetailsAsync(Agenda agenda, CancellationToken cancellationToken = default)
    {
        var entry = AppDbContext.Entry(agenda);
        // Skip references already loaded or fixed up from parents tracked by the handler.
        if (agenda.Workstation is null)
            await entry.Reference(a => a.Workstation).LoadAsync(cancellationToken);
        if (agenda.Location is null)
            await entry.Reference(a => a.Location).LoadAsync(cancellationToken);
        if (agenda.UserCreated is null)
            await entry.Reference(a => a.UserCreated).LoadAsync(cancellationToken);
    }

    private IQueryable<Agenda> WithDetails()
    {
        return AppDbContext.Set<Agenda>()
            .AsNoTracking()
            .Include(a => a.Workstation)
            .Include(a => a.Location)
            .Include(a => a.UserCreated);
    }
}
