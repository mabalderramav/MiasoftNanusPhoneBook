using MiasoftNanus.PhoneBook.Domain.Abstractions;

namespace MiasoftNanus.PhoneBook.Domain.Users.Events;

/// <summary>
/// Represents the domain event that occurs when a user is created.
/// </summary>
/// <remarks>
/// This event is triggered whenever a new user is successfully created
/// in the domain. It carries the unique identifier of the created user.
/// 
/// As part of a domain-driven design implementation, this event can be used
/// to notify other parts of the system about the creation of the user, allowing
/// for actions such as updating other aggregates, sending notifications, or logging.
/// </remarks>
public sealed record UserCreatedDomainEvent(Guid IdUsuario) : IDomainEvent;