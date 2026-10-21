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

    public static Result<User> ToEntity(this CreateUpdateUserDto createUpdateUserDto, DateTime now)
    {
        if (createUpdateUserDto.Address is null)
            return Result.Failure<User>(UserErrors.AddressIsIncomplete);

        return User.Create(new UserCreationData
        {
            FirstName = createUpdateUserDto.FirstName,
            LastName = createUpdateUserDto.LastName,
            Password = createUpdateUserDto.Password,
            Birthdate = createUpdateUserDto.Birthdate,
            Email = createUpdateUserDto.Email,
            Address = new Address(
                createUpdateUserDto.Address.Country,
                createUpdateUserDto.Address.State,
                createUpdateUserDto.Address.Province,
                createUpdateUserDto.Address.District,
                createUpdateUserDto.Address.Street
            ),
            States = createUpdateUserDto.States,
            DateOfLastChange = now,
            ProfileId = createUpdateUserDto.ProfileId
        });
    }
}
