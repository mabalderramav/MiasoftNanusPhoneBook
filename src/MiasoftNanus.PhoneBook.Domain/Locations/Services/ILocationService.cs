using MiasoftNanus.PhoneBook.Domain.Abstractions.Services;
using MiasoftNanus.PhoneBook.Domain.Locations.Entities;

namespace MiasoftNanus.PhoneBook.Domain.Locations.Services;

/// <summary>
/// Provides a contract for managing operations specific to location entities
/// within the domain model. Inherits the generic CRUD operations
/// from <c>ICrudService</c>.
/// </summary>
/// <remarks>
/// The <c>ILocationService</c> interface defines the operations that are
/// specific to managing <c>Location</c> entities. It allows for
/// creating, retrieving, updating, and deleting location data while leveraging
/// the generic CRUD functionalities provided by <c>ICrudService</c>.
/// </remarks>
public interface ILocationService : ICrudService<Location, Guid>;
