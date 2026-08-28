using MediatR;
using MiasoftNanus.PhoneBook.Domain.Abstractions;

namespace MiasoftNanus.PhoneBook.Application.Abstractions.Messaging;

/// <summary>
/// Represents a handler responsible for processing a query and returning a result.
/// </summary>
/// <typeparam name="TQuery">The type of the query that this handler will process.
/// Must implement <see cref="IQuery{TResponse}"/>.</typeparam>
/// <typeparam name="TResponse">The type of the response that the handler will return upon processing
/// the query.</typeparam>
public interface IQueryHandler<in TQuery, TResponse> : IRequestHandler<TQuery, Result<TResponse>>
    where TQuery : IQuery<TResponse>
{
}