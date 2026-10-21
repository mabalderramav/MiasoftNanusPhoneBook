using MiasoftNanus.PhoneBook.Domain.Abstractions.Services;
using MiasoftNanus.PhoneBook.Domain.Workstations.Entities;

namespace MiasoftNanus.PhoneBook.Domain.Workstations.Services;

/// <summary>
/// Provides a contract for managing workstation entities.
/// </summary>
public interface IWorkstationService : ICrudService<Workstation, Guid>;
