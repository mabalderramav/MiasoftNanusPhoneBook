using MediatR;
using MiasoftNanus.PhoneBook.Domain.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace MiasoftNanus.PhoneBook.Infrastructure;

/// <summary>
/// Represents the implementation of a database context used for interacting with
/// the underlying data store. This class is derived from <see cref="DbContext"/>
/// and serves as the entry point for managing and coordinating entity persistence
/// and changes using Entity Framework Core.
/// </summary>
/// <remarks>
/// This class also implements the <see cref="IUnitOfWork"/> interface to provide
/// transactional management of changes across different repositories or domain
/// entities. It integrates the Mediator pattern by using an <see cref="IPublisher"/>
/// to publish domain events after successful database operations.
/// </remarks>
public class AppDbContext(DbContextOptions<AppDbContext> options, IPublisher publisher)
    : DbContext(options), IUnitOfWork
{
    /// <summary>
    /// Configures the entity model using the specified <see cref="ModelBuilder"/>.
    /// This method is called during the initialization of the database context
    /// to set up entity configurations, relationships, and conventions specific
    /// to the application's domain model.
    /// </summary>
    /// <param name="modelBuilder">
    /// An instance of <see cref="ModelBuilder"/> provided by Entity Framework Core
    /// that is used to define and customize the model for the database context.
    /// </param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }

    /// <summary>
    /// Asynchronously saves changes made in the context to the underlying database.
    /// This method also triggers domain events publishing after the changes have been committed,
    /// ensuring that all domain events are propagated correctly following a successful database update.
    /// </summary>
    /// <param name="cancellationToken">
    /// A token that can be used to cancel the save operation before it completes.
    /// If no token is provided, the operation will proceed without the ability to cancel.
    /// </param>
    /// <returns>
    /// A task representing the asynchronous save operation. The task result contains the
    /// number of state entries written to the database.
    /// </returns>
    /// <exception cref="DbUpdateException">
    /// Thrown when an error occurs while saving changes to the database. The exception contains
    /// detailed information regarding the failure.
    /// </exception>
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var result = await base.SaveChangesAsync(cancellationToken);
        await PublishDomainEventAsync(cancellationToken);
        return result;
    }

    /// <summary>
    /// Publishes all domain events associated with changes tracked in the current
    /// database context. This method iterates through all entities with domain events,
    /// invokes their domain events for publishing, and clears the events after publishing.
    /// </summary>
    /// <param name="cancellationToken">
    /// A <see cref="CancellationToken"/> that can be used to propagate notifications
    /// that the operation should be canceled.
    /// </param>
    private async Task PublishDomainEventAsync(CancellationToken cancellationToken)
    {
        var domainEvents = ChangeTracker
            .Entries<Entity>()
            .Select(entity => entity.Entity)
            .SelectMany(e =>
            {
                var domainEvents = e.GetDomainEvents().ToList();
                e.ClearDomainEvents();
                return domainEvents;
            }).ToList();

        foreach (var domainEvent in domainEvents)
        {
            try
            {
                await publisher.Publish(domainEvent, cancellationToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                throw new InvalidOperationException(
                    $"The database transaction was committed successfully, but publishing the '{domainEvent.GetType().Name}' domain event failed.",
                    ex);
            }
        }
    }
}