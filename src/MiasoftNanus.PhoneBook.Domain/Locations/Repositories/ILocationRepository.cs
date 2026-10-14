using MiasoftNanus.PhoneBook.Domain.Abstractions;
using MiasoftNanus.PhoneBook.Domain.Locations.Entities;

namespace MiasoftNanus.PhoneBook.Domain.Locations.Repositories;

/// <summary>
/// Provides an abstraction for the repository that manages location-related operations
/// within the phone book domain. Extends the generic repository interface to
/// handle <see cref="Location"/> entities with unique identifier of type <see cref="Guid"/>.
/// </summary>
public interface ILocationRepository : IGenericRepository<Location, Guid>;