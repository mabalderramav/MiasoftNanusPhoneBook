namespace MiasoftNanus.PhoneBook.Domain.Abstractions;

/// <summary>
/// Represents a marker interface used for notification events within the domain.
/// Implementing this interface indicates that a class is a notification that can typically be handled by a mediator
/// or similar infrastructure for dispatching domain-related events or messages.
/// </summary>
public interface INotification;