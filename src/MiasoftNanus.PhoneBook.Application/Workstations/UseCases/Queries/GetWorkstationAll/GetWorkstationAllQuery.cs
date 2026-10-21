using MiasoftNanus.PhoneBook.Application.Abstractions.CQRS;
using MiasoftNanus.PhoneBook.Domain.Abstractions;
using MiasoftNanus.PhoneBook.Domain.Workstations.Entities;

namespace MiasoftNanus.PhoneBook.Application.Workstations.UseCases.Queries.GetWorkstationAll;

/// <summary>
/// Represents a query to retrieve all workstations.
/// </summary>
public record GetWorkstationAllQuery : IQuery<Result<ICollection<Workstation>>>;
