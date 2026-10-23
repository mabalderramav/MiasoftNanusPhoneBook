using MiasoftNanus.PhoneBook.Domain.Abstractions;
using MiasoftNanus.PhoneBook.Domain.Users.Entities;
using MiasoftNanus.PhoneBook.Domain.Users.Errors;
using MiasoftNanus.PhoneBook.Domain.Users.ObjectValues;
using MiasoftNanus.PhoneBook.WebApi.Users.Dtos;

namespace MiasoftNanus.PhoneBook.WebApi.Users.Mappers;

/// <summary>
/// Provides mapping between user entities and DTOs.
/// </summary>
public static class UserMap
{
    public static UserDto ToDto(this User user)
    {
        return new UserDto(
            user.Id,
            user.FirstName!,
            user.LastName!,
            user.UserName!.Value,
            user.Birthdate,
            user.Email!.Value,
            new AddressDto(
                user.Address!.Country,
                user.Address.State,
                user.Address.Province,
                user.Address.District,
                user.Address.Street
            ),
            user.States!.Value,
            user.DateOfLastChange,
            user.ProfileId
        );
    }

    public static IEnumerable<UserDto> ToDto(this IEnumerable<User> users)
    {
        return users.Select(user => user.ToDto());
    }

    public static Result<User> ToEntity(this CreateUserDto createUserDto, DateTime now)
    {
        if (createUserDto.Address is null)
            return Result.Failure<User>(UserErrors.AddressIsIncomplete);

        return User.Create(new UserCreationData
        {
            FirstName = createUserDto.FirstName,
            LastName = createUserDto.LastName,
            Password = createUserDto.Password,
            Birthdate = createUserDto.Birthdate,
            Email = createUserDto.Email,
            Address = new Address(
                createUserDto.Address.Country,
                createUserDto.Address.State,
                createUserDto.Address.Province,
                createUserDto.Address.District,
                createUserDto.Address.Street
            ),
            States = createUserDto.States,
            DateOfLastChange = now,
            ProfileId = createUserDto.ProfileId
        });
    }

    public static Result<UserData> ToUserData(this UpdateUserDto updateUserDto, DateTime now)
    {
        if (updateUserDto.Address is null)
            return Result.Failure<UserData>(UserErrors.AddressIsIncomplete);

        return Result.Success(new UserData
        {
            FirstName = updateUserDto.FirstName,
            LastName = updateUserDto.LastName,
            Birthdate = updateUserDto.Birthdate,
            Email = updateUserDto.Email,
            Address = new Address(
                updateUserDto.Address.Country,
                updateUserDto.Address.State,
                updateUserDto.Address.Province,
                updateUserDto.Address.District,
                updateUserDto.Address.Street
            ),
            States = updateUserDto.States,
            DateOfLastChange = now,
            ProfileId = updateUserDto.ProfileId
        });
    }
}
