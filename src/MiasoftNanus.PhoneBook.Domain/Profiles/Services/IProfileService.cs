using MiasoftNanus.PhoneBook.Domain.Abstractions;
using MiasoftNanus.PhoneBook.Domain.Abstractions.Services;
using MiasoftNanus.PhoneBook.Domain.Profiles.Entities;

namespace MiasoftNanus.PhoneBook.Domain.Profiles.Services;

/// <summary>
/// Provides a contract for managing operations specific to profile entities
/// within the domain model. Inherits the generic CRUD operations
/// from <c>ICrudService</c>.
/// </summary>
/// <remarks>
/// The <c>IProfileService</c> interface defines the operations that are
/// specific to managing <c>Profile</c> entities. It allows for
/// creating, retrieving, updating, and deleting profile data while leveraging
/// the generic CRUD functionalities provided by <c>ICrudService</c>.
/// </remarks>
public interface IProfileService : ICrudService<Profile, Guid>;