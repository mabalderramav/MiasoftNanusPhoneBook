namespace MiasoftNanus.PhoneBook.Domain.Users.ObjectValues;

/// <summary>
/// Represents an address with multiple location components,
/// such as country, department, province, district, and street.
/// </summary>
public record Address(
    string Country,
    string Department,
    string Province,
    string District,
    string Street
);