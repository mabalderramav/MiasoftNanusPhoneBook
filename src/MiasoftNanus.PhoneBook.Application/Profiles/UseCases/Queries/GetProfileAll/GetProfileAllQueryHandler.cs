using MiasoftNanus.PhoneBook.Application.Abstractions.CQRS;
using MiasoftNanus.PhoneBook.Domain.Abstractions;
using MiasoftNanus.PhoneBook.Domain.Profiles.Entities;
using MiasoftNanus.PhoneBook.Domain.Profiles.Errors;
using MiasoftNanus.PhoneBook.Domain.Profiles.Repositories;

namespace MiasoftNanus.PhoneBook.Application.Profiles.UseCases.Queries.GetProfileAll;

/// <summary>
/// Handles the query to retrieve all the profiles.
/// Implements the <c>IQueryHandler</c> interface to process the <c>GetProfileAllQuery</c>
/// and return the result wrapped as <c>Result&lt;ICollection&lt;Profile&gt;&gt;</c>.
/// </summary>
/// <remarks>
/// This handler depends on the <c>IProfileRepository</c> to fetch the profiles from the data source.
/// It returns a success result if profiles are found; otherwise, it returns a failure result.
/// </remarks>
public class GetProfileAllQueryHandler(IProfileRepository profileRepository) :
    IQueryHandler<GetProfileAllQuery, Result<ICollection<Profile>>>
{
    public async Task<Result<ICollection<Profile>>> HandleAsync(GetProfileAllQuery query,
        CancellationToken cancellationToken = default)
    {
        var profiles = await profileRepository.GetAllAsync(cancellationToken);
        return profiles.Count > 0
            ? Result.Success(profiles)
            : Result.Failure<ICollection<Profile>>(ProfileErrors.ProfilesNotFound);
    }
}