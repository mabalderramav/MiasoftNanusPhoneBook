namespace MiasoftNanus.PhoneBook.Domain.Abstractions;

/// <summary>
/// Represents the base class for entities in a domain-driven design context.
/// </summary>
/// <remarks>
/// The <c>Entity</c> class serves as a base for domain entities, providing a unique
/// identifier and support for managing domain events.
/// 
/// Domain entities are objects within the domain model that typically have an identity
/// that is consistent and unique throughout the system. This class encapsulates common
/// functionality such as managing domain events, which allows for better encapsulation
/// and separation of concerns.
/// 
/// Derived classes should inherit from this class to leverage its features, such as
/// handling domain events and maintaining consistent entity identification.
/// </remarks>
public abstract class Entity
{
    /// <summary>
    /// Gets the unique identifier for the entity.
    /// </summary>
    /// <remarks>
    /// The <c>Id</c> property serves as the primary key for the entity, ensuring that each instance
    /// can be uniquely identified across the system. It is assigned a value during the creation
    /// of the entity and remains immutable.
    /// </remarks>
    public Guid Id { get; init; }

    private readonly List<IDomainEvent> _domainEvents = [];

    /// <summary>
    /// Represents the base class for entities in a domain-driven design context.
    /// </summary>
    /// <remarks>
    /// The <c>Entity</c> class provides a unique identifier and support for domain events,
    /// enabling encapsulation of behavior typically associated with domain entities.
    /// It is designed to be inherited by other classes that require these features.
    /// </remarks>
    protected Entity(Guid id)
    {
        Id = id;
    }

    /// <summary>
    /// Represents the base class for entities in a domain-driven design context.
    /// </summary>
    /// <remarks>
    /// The <c>Entity</c> class serves as the foundation for domain entities by providing a
    /// unique identifier and support for managing domain events. It is designed for inheritance
    /// by domain-specific entities to enable encapsulation and consistent handling of behaviors
    /// related to domain events and entity identification.
    /// 
    /// This class plays a key role in domain-driven design by ensuring that entities maintain
    /// a unique identity throughout their lifecycle and facilitating the management of domain
    /// events, which contribute to maintaining separation of concerns in the domain layer.
    /// </remarks>
    protected Entity()
    {
    }

    /// <summary>
    /// Adds a domain event to the entity's event collection for later processing.
    /// </summary>
    /// <param name="domainEvent">
    /// The domain event to be added. This represents an action or occurrence
    /// that has significance within the domain context.
    /// </param>
    protected void AddDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    /// <summary>
    /// Clears all domain events from the entity's event collection.
    /// </summary>
    /// <remarks>
    /// This method removes all domain events that have been added to the entity's
    /// collection. It is typically used after domain events have been processed
    /// to reset the state of the event collection.
    /// </remarks>
    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }

    /// <summary>
    /// Retrieves the collection of domain events associated with the entity.
    /// </summary>
    /// <returns>
    /// An <see cref="IReadOnlyCollection{T}"/> containing the domain events. This ensures that the events
    /// are exposed in a read-only manner, preserving the integrity of the internal collection.
    /// </returns>
    public IReadOnlyCollection<IDomainEvent> GetDomainEvents() => _domainEvents.AsReadOnly();
}