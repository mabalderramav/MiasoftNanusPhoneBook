using MiasoftNanus.PhoneBook.Application.Abstractions.CQRS;
using MiasoftNanus.PhoneBook.Domain.Abstractions;
using MiasoftNanus.PhoneBook.Domain.WorkAreas.Entities;

namespace MiasoftNanus.PhoneBook.Application.WorkAreas.UseCases.Queries.GetWorkAreaById;

/// <summary>
/// Represents a query to retrieve a work area by identifier.
/// </summary>
public record GetWorkAreaByIdQuery(Guid Id) : IQuery<Result<WorkArea>>;
