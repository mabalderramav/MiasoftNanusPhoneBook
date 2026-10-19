using System.Collections.Concurrent;
using MiasoftNanus.PhoneBook.Domain.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace MiasoftNanus.PhoneBook.Application.Abstractions.Notifications;

/// <summary>
/// The <c>Publisher</c> class is responsible for publishing notifications to their respective handlers.
/// </summary>
/// <remarks>
/// This class implements the <see cref="IPublisher"/> interface and serves as a concrete implementation
/// of a notification publishing mechanism. It uses a caching mechanism to store and reuse notification invokers
/// for each notification type to optimize performance.
/// </remarks>
/// <example>
/// This class does not contain an example usage inside its documentation.
/// </example>
/// <param name="serviceProvider">
/// The <see cref="IServiceProvider"/> instance used to resolve the required services for processing notifications.
/// </param>
/// <exception cref="ArgumentNullException">
/// Thrown when the notification event being published is null.
/// </exception>
/// <threadsafety>
/// This class is not guaranteed to be thread-safe.
/// </threadsafety>
internal sealed class Publisher(IServiceProvider serviceProvider) : IPublisher
{
    private static readonly ConcurrentDictionary<Type, INotificationInvoker> InvokerCache = new();

    public Task PublishAsync<TNotification>(
        TNotification @event,
        CancellationToken cancellationToken = default)
        where TNotification : INotification
    {
        ArgumentNullException.ThrowIfNull(@event);

        var notificationType = @event.GetType();
        var invoker = InvokerCache.GetOrAdd(
            notificationType,
            static type => (INotificationInvoker)Activator.CreateInstance(
                typeof(NotificationInvoker<>).MakeGenericType(type))!);

        return invoker.InvokeAsync(@event, serviceProvider, cancellationToken);
    }

    /// <summary>
    /// Represents an abstraction for invoking notifications within the notification dispatching process.
    /// </summary>
    /// <remarks>
    /// The <c>INotificationInvoker</c> is responsible for executing the necessary actions to handle a given
    /// notification. It provides a consistent mechanism to process notifications using dependency injection
    /// to resolve any required services.
    /// This interface is commonly used by notification dispatchers or publishers to abstract the invocation details.
    /// </remarks>
    /// <threadsafety>
    /// Implementations of this interface are not guaranteed to be thread-safe.
    /// </threadsafety>
    /// <exception cref="ArgumentNullException">
    /// Thrown when required notification or service arguments are null during invocation.
    /// </exception>
    private interface INotificationInvoker
    {
        Task InvokeAsync(
            INotification notification,
            IServiceProvider services,
            CancellationToken cancellationToken);
    }

    /// <summary>
    /// The <c>NotificationInvoker</c> class is responsible for invoking notification handlers
    /// that process a specific type of notification.
    /// </summary>
    /// <typeparam name="TNotification">
    /// The type of the notification being handled. This type must implement <see cref="INotification"/>.
    /// </typeparam>
    /// <remarks>
    /// This class is a concrete implementation of the <c>INotificationInvoker</c> interface
    /// and is designed to resolve and invoke all registered notification handlers for the specified
    /// notification type. It relies on the <see cref="IServiceProvider"/> to resolve dependencies.
    /// </remarks>
    /// <exception cref="InvalidCastException">
    /// Thrown when the notification passed to the <c>InvokeAsync</c> method cannot be cast
    /// to the specified notification type <c>TNotification</c>.
    /// </exception>
    /// <threadsafety>
    /// This class assumes that the notification handlers being invoked are thread-safe. However, the class
    /// itself does not guarantee thread safety when shared across multiple threads.
    /// </threadsafety>
    /// <seealso cref="INotification"/>
    /// <seealso cref="INotificationHandler{TNotification}"/>
    private sealed class NotificationInvoker<TNotification> : INotificationInvoker
        where TNotification : INotification
    {
        public async Task InvokeAsync(
            INotification notification,
            IServiceProvider services,
            CancellationToken cancellationToken)
        {
            foreach (var handler in services.GetServices<INotificationHandler<TNotification>>())
            {
                await handler.HandleAsync((TNotification)notification, cancellationToken);
            }
        }
    }
}