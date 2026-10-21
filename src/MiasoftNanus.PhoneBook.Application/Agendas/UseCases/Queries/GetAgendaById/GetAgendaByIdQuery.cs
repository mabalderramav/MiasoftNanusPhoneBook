using MiasoftNanus.PhoneBook.Application.Abstractions.CQRS;
using MiasoftNanus.PhoneBook.Domain.Abstractions;
using MiasoftNanus.PhoneBook.Domain.Agendas.Entities;

namespace MiasoftNanus.PhoneBook.Application.Agendas.UseCases.Queries.GetAgendaById;

/// <summary>
/// Represents a query to retrieve an agenda by identifier.
/// </summary>
public record GetAgendaByIdQuery(Guid Id) : IQuery<Result<Agenda>>;
