using MiasoftNanus.PhoneBook.Domain.Abstractions;

namespace MiasoftNanus.PhoneBook.Domain.Agendas.Events;

/// <summary>
/// Represents a domain event triggered when a new agenda is created.
/// </summary>
/// <remarks>
/// This event contains the identifier of the newly created agenda as its primary data.
/// It is used to notify other parts of the system about the agenda creation, enabling
/// decoupled components to react to this occurrence if necessary.
/// 
/// The <see cref="IDomainEvent"/> is implemented to ensure compatibility with
/// domain-driven design principles and to leverage event-handling mechanisms such as MediatR.
/// </remarks>
/// <param name="AgendaId">
/// The unique identifier of the agenda that was created.
/// </param>
public sealed record AgendaCreatedDomainEvent(Guid AgendaId) : IDomainEvent;