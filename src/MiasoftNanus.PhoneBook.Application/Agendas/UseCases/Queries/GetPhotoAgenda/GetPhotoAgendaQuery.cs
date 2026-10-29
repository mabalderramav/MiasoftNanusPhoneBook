using MiasoftNanus.PhoneBook.Application.Abstractions.CQRS;
using MiasoftNanus.PhoneBook.Domain.Abstractions;
using MiasoftNanus.PhoneBook.Domain.Agendas.Entities;

namespace MiasoftNanus.PhoneBook.Application.Agendas.UseCases.Queries.GetPhotoAgenda;

/// <summary>
/// Represents a query to retrieve the photo of an agenda.
/// </summary>
public record GetPhotoAgendaQuery(Guid Id) : IQuery<Result<AgendaPhoto>>;
