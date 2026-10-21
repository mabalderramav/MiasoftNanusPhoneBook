using MiasoftNanus.PhoneBook.Application.Abstractions.CQRS;
using MiasoftNanus.PhoneBook.Domain.Abstractions;
using MiasoftNanus.PhoneBook.Domain.Agendas.Entities;

namespace MiasoftNanus.PhoneBook.Application.Agendas.UseCases.Queries.GetAgendaAll;

/// <summary>
/// Represents a query to retrieve all agendas.
/// </summary>
public record GetAgendaAllQuery : IQuery<Result<ICollection<Agenda>>>;
