using MediatR;
using MiasoftNanus.PhoneBook.Domain.Abstractions;

namespace MiasoftNanus.PhoneBook.Application.Abstractions.Messaging;

/// <summary>
/// Represents a marker interface for a query that encapsulates the request for data
/// and expects a response of type <typeparamref name="TResponse"/>.
/// </summary>
/// <typeparam name="TResponse">The type of the response expected from executing the query.</typeparam>
public interface IQuery<TResponse> : IRequest<Result<TResponse>>
{
}