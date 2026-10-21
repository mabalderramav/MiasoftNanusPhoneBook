using MiasoftNanus.PhoneBook.Domain.Abstractions;

namespace MiasoftNanus.PhoneBook.Domain.Users.Errors;

/// <summary>
/// Provides a collection of predefined errors specific to the User domain.
/// These errors can be used for validation and error handling when working with User entities.
/// </summary>
public static class UserErrors
{
    public static Error UserNotFound =>
        new Error("UserErrors.UserNotFound", "The user was not found."
        );

    public static Error UsersNotFound =>
        new Error("UserErrors.UsersNotFound", "The users were not found."
        );

    public static Error FirstNameCannotBeNullOrWhitespace =>
        new Error("UserErrors.FirstNameCannotBeNullOrWhitespace", "First name cannot be null or whitespace."
        );

    public static Error LastNameCannotBeNullOrWhitespace =>
        new Error("UserErrors.LastNameCannotBeNullOrWhitespace", "Last name cannot be null or whitespace."
        );

    public static Error InvalidPassword =>
        new Error("UserErrors.InvalidPassword", "The provided password is invalid."
        );

    public static Error InvalidEmail =>
        new Error("UserErrors.InvalidEmail", "The provided email is invalid."
        );

    public static Error BirthdateIsRequired =>
        new Error("UserErrors.BirthdateIsRequired", "Birthdate is required."
        );

    public static Error AddressIsIncomplete =>
        new Error("UserErrors.AddressIsIncomplete",
            "Address country, state, province, district and street are required."
        );

    public static Error InvalidState =>
        new Error("UserErrors.InvalidState", "The provided state is invalid."
        );

    public static Error ProfileIdCannotBeEmpty =>
        new Error("UserErrors.ProfileIdCannotBeEmpty", "Profile id cannot be empty."
        );
}
