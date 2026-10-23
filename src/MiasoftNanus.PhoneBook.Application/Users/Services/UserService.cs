using MiasoftNanus.PhoneBook.Application.Abstractions.CQRS;
using MiasoftNanus.PhoneBook.Application.Abstractions.Services;
using MiasoftNanus.PhoneBook.Application.Users.UseCases.Commands.ChangeUserPassword;
using MiasoftNanus.PhoneBook.Application.Users.UseCases.Commands.CreateUser;
using MiasoftNanus.PhoneBook.Application.Users.UseCases.Commands.DeleteUser;
using MiasoftNanus.PhoneBook.Application.Users.UseCases.Commands.UpdateUser;
using MiasoftNanus.PhoneBook.Application.Users.UseCases.Queries.GetUserAll;
using MiasoftNanus.PhoneBook.Application.Users.UseCases.Queries.GetUserById;
using MiasoftNanus.PhoneBook.Domain.Abstractions;
using MiasoftNanus.PhoneBook.Domain.Users.Entities;
using MiasoftNanus.PhoneBook.Domain.Users.Services;

namespace MiasoftNanus.PhoneBook.Application.Users.Services;

/// <summary>
/// Provides CRUD functionality for users through the CQRS dispatcher.
/// </summary>
public class UserService(IDispatcher dispatcher)
    : CrudCqrsService<User, Guid>(dispatcher), IUserService
{
    protected override ICommand<Result<User>> BuildCreateCommand(User entity)
    {
        return new CreateUserCommand(
            entity.FirstName!,
            entity.LastName!,
            entity.Password!.Value,
            entity.Birthdate,
            entity.Email!.Value,
            entity.Address!.Country,
            entity.Address.State,
            entity.Address.Province,
            entity.Address.District,
            entity.Address.Street,
            entity.States!.Value,
            entity.ProfileId);
    }

    protected override IQuery<Result<User>> BuildGetByIdQuery(Guid id)
    {
        return new GetUserByIdQuery(id);
    }

    protected override IQuery<Result<ICollection<User>>> BuildGetAllQuery()
    {
        return new GetUserAllQuery();
    }

    protected override ICommand<Result<User>> BuildUpdateCommand(Guid id, User entity)
    {
        return new UpdateUserCommand(
            id,
            entity.FirstName!,
            entity.LastName!,
            entity.Birthdate,
            entity.Email!.Value,
            entity.Address!.Country,
            entity.Address.State,
            entity.Address.Province,
            entity.Address.District,
            entity.Address.Street,
            entity.States!.Value,
            entity.ProfileId);
    }

    protected override ICommand<Result> BuildDeleteCommand(Guid id)
    {
        return new DeleteUserCommand(id);
    }

    public Task<Result<User>> UpdateDetailsAsync(
        Guid id,
        UserData data,
        CancellationToken cancellationToken = default)
    {
        var command = new UpdateUserCommand(
            id,
            data.FirstName,
            data.LastName,
            data.Birthdate,
            data.Email,
            data.Address.Country,
            data.Address.State,
            data.Address.Province,
            data.Address.District,
            data.Address.Street,
            data.States,
            data.ProfileId);

        return Dispatcher.SendCommandAsync(command, cancellationToken);
    }

    public Task<Result> ChangePasswordAsync(Guid id, string password, CancellationToken cancellationToken = default)
    {
        return Dispatcher.SendCommandAsync(new ChangeUserPasswordCommand(id, password), cancellationToken);
    }
}
