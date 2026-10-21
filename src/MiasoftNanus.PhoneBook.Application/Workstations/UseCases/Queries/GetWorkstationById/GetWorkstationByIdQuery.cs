using MiasoftNanus.PhoneBook.Application.Abstractions.CQRS;
using MiasoftNanus.PhoneBook.Domain.Abstractions;
using MiasoftNanus.PhoneBook.Domain.Workstations.Entities;

namespace MiasoftNanus.PhoneBook.Application.Workstations.UseCases.Queries.GetWorkstationById;

/// <summary>
/// Represents a query to retrieve a workstation by identifier.
/// </summary>
public record GetWorkstationByIdQuery(Guid Id) : IQuery<Result<Workstation>>;
