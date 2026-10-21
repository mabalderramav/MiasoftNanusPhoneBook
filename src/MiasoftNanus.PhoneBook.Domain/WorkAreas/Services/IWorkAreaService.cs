using MiasoftNanus.PhoneBook.Domain.Abstractions.Services;
using MiasoftNanus.PhoneBook.Domain.WorkAreas.Entities;

namespace MiasoftNanus.PhoneBook.Domain.WorkAreas.Services;

/// <summary>
/// Provides a contract for managing work area entities.
/// </summary>
public interface IWorkAreaService : ICrudService<WorkArea, Guid>;
