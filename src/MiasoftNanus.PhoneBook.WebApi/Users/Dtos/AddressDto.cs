namespace MiasoftNanus.PhoneBook.WebApi.Users.Dtos;

/// <summary>
/// Data transfer object representing a user address.
/// </summary>
public record AddressDto(
    string Country,
    string State,
    string Province,
    string District,
    string Street
);
