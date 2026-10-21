using MiasoftNanus.PhoneBook.Application.Abstractions.CQRS;
using MiasoftNanus.PhoneBook.Domain.Abstractions;
using MiasoftNanus.PhoneBook.Domain.WorkAreas.Entities;

namespace MiasoftNanus.PhoneBook.Application.WorkAreas.UseCases.Queries.GetWorkAreaAll;

/// <summary>
/// Represents a query to retrieve all work areas.
/// </summary>
public record GetWorkAreaAllQuery : IQuery<Result<ICollection<WorkArea>>>;
