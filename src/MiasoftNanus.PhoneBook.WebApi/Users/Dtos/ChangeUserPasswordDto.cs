namespace MiasoftNanus.PhoneBook.WebApi.Users.Dtos;

/// <summary>
/// Data transfer object used for changing the password of a user.
/// </summary>
public record ChangeUserPasswordDto(string Password)
{
    // Masks the raw password so it never leaks through logs.
    public override string ToString() => $"{nameof(ChangeUserPasswordDto)} {{ Password = ***** }}";
}
