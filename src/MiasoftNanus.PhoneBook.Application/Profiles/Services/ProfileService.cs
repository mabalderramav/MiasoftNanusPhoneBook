using MiasoftNanus.PhoneBook.Application.Abstractions.CQRS;
using MiasoftNanus.PhoneBook.Application.Abstractions.Services;
using MiasoftNanus.PhoneBook.Application.Profiles.UseCases.Commands.CreateProfile;
using MiasoftNanus.PhoneBook.Application.Profiles.UseCases.Commands.DeleteProfile;
using MiasoftNanus.PhoneBook.Application.Profiles.UseCases.Commands.UpdateProfile;
using MiasoftNanus.PhoneBook.Application.Profiles.UseCases.Queries.GetProfileAll;
using MiasoftNanus.PhoneBook.Application.Profiles.UseCases.Queries.GetProfileById;
using MiasoftNanus.PhoneBook.Domain.Abstractions;
using MiasoftNanus.PhoneBook.Domain.Profiles.Entities;
using MiasoftNanus.PhoneBook.Domain.Profiles.Services;

namespace MiasoftNanus.PhoneBook.Application.Profiles.Services;

/// <summary>
/// Provides functionality for managing profiles within the application.
/// </summary>
/// <remarks>
/// This service extends the base functionality provided by <see cref="CrudCqrsService{T, TId}"/>
/// and implements operations specific to profile entities. It interacts with the command
/// and query dispatcher to perform CRUD operations for profiles.
/// </remarks>
/// <inheritdoc cref="CrudCqrsService{T, TId}" />
public class ProfileService(IDispatcher dispatcher) : CrudCqrsService<Profile, Guid>(dispatcher), IProfileService
{
    protected override ICommand<Result<Profile>> BuildCreateCommand(Profile entity)
    {
        return new CreateProfileCommand(entity.ProfileName, entity.Description);
    }

    protected override IQuery<Result<Profile>> BuildGetByIdQuery(Guid id)
    {
        return new GetProfileByIdQuery(id);
    }

    protected override IQuery<Result<ICollection<Profile>>> BuildGetAllQuery()
    {
        return new GetProfileAllQuery();
    }

    protected override ICommand<Result<Profile>> BuildUpdateCommand(Profile entity)
    {
        return new UpdateProfileCommand(entity.Id, entity.ProfileName, entity.Description);
    }

    protected override ICommand<Result> BuildDeleteCommand(Profile entity)
    {
        return new DeleteProfileCommand(entity.Id);
    }
}