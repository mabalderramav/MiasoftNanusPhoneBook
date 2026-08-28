using MiasoftNanus.PhoneBook.Application.Abstractions.Messaging;

namespace MiasoftNanus.PhoneBook.Application.Users.GetUserById;

/// <summary>
/// Represents a query to retrieve a user by their unique identifier.
/// </summary>
/// <remarks>
/// This query is used to request detailed user information specified by a GUID.
/// It encapsulates the unique identifier of the user and is designed to be handled by
/// a query handler that processes the request and provides the corresponding user data
/// in the form of a <c>GetUserByIdResult</c>.
/// </remarks>
public record GetUserByIdQuery(Guid Id) : IQuery<GetUserByIdResult>;