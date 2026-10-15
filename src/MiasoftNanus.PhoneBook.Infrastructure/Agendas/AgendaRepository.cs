using MiasoftNanus.PhoneBook.Domain.Agendas.Entities;
using MiasoftNanus.PhoneBook.Domain.Agendas.Repositories;
using MiasoftNanus.PhoneBook.Infrastructure.Shared.Repositories;

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
public class AgendaRepository(AppDbContext appDbContext) : CrudRepository<Agenda>(appDbContext), IAgendaRepository;