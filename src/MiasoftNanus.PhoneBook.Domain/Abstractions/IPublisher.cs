namespace MiasoftNanus.PhoneBook.Domain.Abstractions;

/// <summary>
/// Represents a contract for publishing notifications in a mediated communication model.
/// </summary>
/// <remarks>
/// The IPublisher interface is intended to be used for publishing domain events or
/// notifications as part of a publisher-subscriber pattern. Implementations of this
/// interface interact with notification handlers to distribute events across the application.
/// This is useful in decoupling the event publishers from the eventual consumers of the
/// notifications, enhancing modularity and testability.
/// </remarks>
public interface IPublisher
{
    /// <summary>
    /// Publishes a notification event to all relevant handlers.
    /// </summary>
    /// <typeparam name="TNotification">The type of the notification being published.
    /// Must implement <see cref="INotification"/>.</typeparam>
    /// <param name="event">The notification event to publish.</param>
    /// <param name="cancellationToken">An optional token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task PublishAsync<TNotification>(TNotification @event, CancellationToken cancellationToken = default)
        where TNotification : INotification;
}