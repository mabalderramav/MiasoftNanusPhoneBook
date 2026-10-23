using MiasoftNanus.PhoneBook.Application.Users.UseCases.Commands.CreateUser;
using MiasoftNanus.PhoneBook.Application.Users.UseCases.Commands.UpdateUser;
using MiasoftNanus.PhoneBook.Domain.Users.Entities;
using MiasoftNanus.PhoneBook.Domain.Users.ObjectValues;

namespace MiasoftNanus.PhoneBook.Application.Users.Mappers;

/// <summary>
/// Provides mapping from user commands to <see cref="UserCreationData"/> and <see cref="UserData"/>.
/// </summary>
public static class UserCommandMapper
{
    public static UserCreationData ToUserCreationData(this CreateUserCommand command, DateTime now)
    {
        return new UserCreationData
        {
            FirstName = command.FirstName,
            LastName = command.LastName,
            Password = command.Password,
            Birthdate = command.Birthdate,
            Email = command.Email,
            Address = new Address(command.Country, command.State, command.Province, command.District, command.Street),
            States = command.States,
            DateOfLastChange = now,
            ProfileId = command.ProfileId
        };
    }

    public static UserData ToUserData(this UpdateUserCommand command, DateTime now)
    {
        return new UserData
        {
            FirstName = command.FirstName,
            LastName = command.LastName,
            Birthdate = command.Birthdate,
            Email = command.Email,
            Address = new Address(command.Country, command.State, command.Province, command.District, command.Street),
            States = command.States,
            DateOfLastChange = now,
            ProfileId = command.ProfileId
        };
    }
}
