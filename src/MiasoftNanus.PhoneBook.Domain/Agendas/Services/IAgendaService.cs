using MiasoftNanus.PhoneBook.Domain.Abstractions.Services;
using MiasoftNanus.PhoneBook.Domain.Agendas.Entities;

namespace MiasoftNanus.PhoneBook.Domain.Agendas.Services;

/// <summary>
/// Provides a contract for managing agenda entities.
/// </summary>
public interface IAgendaService : ICrudService<Agenda, Guid>;
