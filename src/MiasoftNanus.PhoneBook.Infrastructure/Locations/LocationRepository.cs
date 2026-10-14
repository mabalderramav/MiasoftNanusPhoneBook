using MiasoftNanus.PhoneBook.Domain.Locations.Entities;
using MiasoftNanus.PhoneBook.Domain.Locations.Repositories;
using MiasoftNanus.PhoneBook.Infrastructure.Shared.Repositories;

namespace MiasoftNanus.PhoneBook.Infrastructure.Locations;

/// <summary>
/// Provides repository functionality for managing <see cref="Location"/> entities.
/// </summary>
/// <remarks>
/// This repository is used to perform CRUD operations on <see cref="Location"/> entities
/// using the underlying <see cref="AppDbContext"/>. It inherits common repository
/// functionality from <see cref="CrudRepository{T}"/> and implements the
/// <see cref="ILocationRepository"/> interface for additional location-specific operations.
/// </remarks>
public class LocationRepository(AppDbContext appDbContext) :
    CrudRepository<Location>(appDbContext), ILocationRepository;