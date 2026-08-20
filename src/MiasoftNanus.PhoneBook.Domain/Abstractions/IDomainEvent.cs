using MediatR;

namespace MiasoftNanus.PhoneBook.Domain.Abstractions;

/// <summary>
/// Represents a domain event in a domain-driven design context.
/// </summary>
/// <remarks>
/// Domain events are notifications about occurrences in the domain that have significance
/// to other parts of the system. They are typically used to ensure decoupling between different
/// components or aggregates within the domain.
/// 
/// Implementations of this interface can be used to describe specific events that occur
/// in the domain, and these events can be published or handled using mediator patterns
/// or other event-handling mechanisms.
/// 
/// This interface extends the INotification interface from MediatR, providing compatibility
/// with the MediatR library for handling and dispatching notifications in the system.
/// </remarks>
public interface IDomainEvent  : INotification;