using MiasoftNanus.PhoneBook.Application.Abstractions.CQRS;
using MiasoftNanus.PhoneBook.Domain.Abstractions;
using MiasoftNanus.PhoneBook.Domain.Profiles.Entities;
using MiasoftNanus.PhoneBook.Domain.Profiles.Errors;
using MiasoftNanus.PhoneBook.Domain.Profiles.Repositories;

namespace MiasoftNanus.PhoneBook.Application.Profiles.UseCases.Queries.GetProfileById;

/// <summary>
/// Handles the query to retrieve a profile by its unique identifier.
/// Implements the <c>IQueryHandler</c> interface to process the <c>GetProfileByIdQuery</c>
/// and return the result wrapped as <c>Result&lt;Profile&gt;</c>.
/// </summary>
/// <remarks>
/// This handler depends on the <c>IProfileRepository</c> to fetch the profile data from the data source.
/// If a profile with the specified identifier is found, it returns a success result containing the profile instance.
/// Otherwise, a failure result is returned, indicating that the profile was not found.
/// </remarks>
public class GetProfileByIdQueryHandler(IProfileRepository profileRepository ) :
    IQueryHandler<GetProfileByIdQuery, Result<Profile>>
{
    public async Task<Result<Profile>> HandleAsync(GetProfileByIdQuery query, CancellationToken cancellationToken = default)
    {
        var profile = await profileRepository.GetByIdAsync(query.Id, cancellationToken);
        return profile is not null ? Result.Success(profile) : Result.Failure<Profile>(ProfileErrors.ProfileNotFound);
    }
}