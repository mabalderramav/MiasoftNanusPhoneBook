using MiasoftNanus.PhoneBook.Domain.Abstractions;

namespace MiasoftNanus.PhoneBook.Domain.Users.Errors;

/// <summary>
/// Provides a collection of predefined errors related to user operations.
/// </summary>
public static class UserErrors
{
    public static Error UserNotFound => 
        new("User.NotFound", "The user was not found.");
    public static Error InvalidPassword => 
        new("User.InvalidPassword", "The provided password is invalid.");
    public static Error InvalidEmail => 
        new("User.InvalidEmail", "The provided email is invalid.");
}