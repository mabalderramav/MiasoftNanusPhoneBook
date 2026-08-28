using MiasoftNanus.PhoneBook.Application.Abstractions.Messaging;
using MiasoftNanus.PhoneBook.Application.Abstractions.Time;
using MiasoftNanus.PhoneBook.Domain.Abstractions;
using MiasoftNanus.PhoneBook.Domain.Profiles.Errors;
using MiasoftNanus.PhoneBook.Domain.Profiles.Repositories;
using MiasoftNanus.PhoneBook.Domain.Users.Entities;
using MiasoftNanus.PhoneBook.Domain.Users.ObjectValues;
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

        var passwordResult = Password.Create(request.Password);

        if (passwordResult.IsFailure)
            return Result.Failure<Guid>(passwordResult.Error);

        var emailResult = Email.Create(request.Email);

        if (emailResult.IsFailure)
            return Result.Failure<Guid>(emailResult.Error);
        var addressResult = new Address(
            request.Country,
            request.State,
            request.Province,
            request.District,
            request.Street
        );
        var userDataCreation = new UserCreationData()
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Password = passwordResult.Value,
            Birthdate = request.Birthdate.ToUniversalTime(),
            Email = emailResult.Value,
            Address = addressResult,
            States = request.States,
            DateOfLastChange = dateTimeProvider.CurrentTime,
            ProfileId = profileResult.Id
        };
        var usuarioResult = User.Create(userDataCreation);

        if (usuarioResult.IsFailure)
            return Result.Failure<Guid>(usuarioResult.Error);

        userRepository.Add(usuarioResult.Value);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(usuarioResult.Value.Id);
    }
}