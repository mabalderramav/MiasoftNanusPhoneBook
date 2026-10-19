namespace MiasoftNanus.PhoneBook.Domain.Abstractions;

/// <summary>
/// Handles notifications published by the application.
/// </summary>
/// <typeparam name="TNotification">The notification type to handle.</typeparam>
public interface INotificationHandler<in TNotification>
    where TNotification : INotification
{
    /// <summary>
    /// Handles a published notification.
    /// </summary>
    Task HandleAsync(TNotification notification, CancellationToken cancellationToken = default);
}
