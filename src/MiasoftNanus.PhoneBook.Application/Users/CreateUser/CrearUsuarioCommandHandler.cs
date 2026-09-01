using MiasoftNanus.PhoneBook.Application.Abstractions.Messaging;
using MiasoftNanus.PhoneBook.Application.Abstractions.Time;
using MiasoftNanus.PhoneBook.Domain.Abstractions;
using MiasoftNanus.PhoneBook.Domain.Profiles.Errors;
using MiasoftNanus.PhoneBook.Domain.Profiles.Repositories;
using MiasoftNanus.PhoneBook.Domain.Users.Entities;
using ObjectValues = MiasoftNanus.PhoneBook.Domain.Users.ObjectValues;
using MiasoftNanus.PhoneBook.Domain.Users.Repositories;

namespace MiasoftNanus.PhoneBook.Application.Users.CreateUser;

/// <summary>
/// Handles the execution of the <see cref="CreateUserCommand"/>.
/// This class is responsible for managing the creation of a new user,
/// including interactions with the user and profile repositories,
/// managing transactional consistency, and handling date-time operations.
/// </summary>
/// <remarks>
/// The <see cref="CreateUserCommandHandler"/> uses the following dependencies:
/// <list type="bullet">
/// <item>
/// <term>IUserRepository</term>
/// <description>Provides access to user-related data and operations in the persistence layer.</description>
/// </item>
/// <item>
/// <term>IProfileRepository</term>
/// <description>Handles operations related to user profiles within the persistence layer.</description>
/// </item>
/// <item>
/// <term>IUnitOfWork</term>
/// <description>Manages transactional consistency across repositories.</description>
/// </item>
/// <item>
/// <term>IDateTimeProvider</term>
/// <description>Supplies the current date and time, abstracted for better testability.</description>
/// </item>
/// </list>
/// </remarks>
internal sealed class CreateUserCommandHandler(
    IUserRepository userRepository,
    IProfileRepository profileRepository,
    IUnitOfWork unitOfWork,
    IDateTimeProvider dateTimeProvider)
    : ICommandHandler<CreateUserCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        var profileResult = await profileRepository.GetByNameAsync(request.RoleName, cancellationToken);

        if (profileResult is null)
            return Result.Failure<Guid>(ProfileErrors.ProfileNotFound);

        var passwordResult = ObjectValues.Password.Create(request.Password);

        if (passwordResult.IsFailure)
            return Result.Failure<Guid>(passwordResult.Error);

        var emailResult = ObjectValues.Email.Create(request.Email);

        if (emailResult.IsFailure)
            return Result.Failure<Guid>(emailResult.Error);
        
        var userDataCreation = request.MapToUserCreationData(
            passwordResult.Value, 
            emailResult.Value, 
            profileResult, 
            dateTimeProvider);
        
        var usuarioResult = User.Create(userDataCreation);

        if (usuarioResult.IsFailure)
            return Result.Failure<Guid>(usuarioResult.Error);

        userRepository.Add(usuarioResult.Value);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(usuarioResult.Value.Id);
    }
}