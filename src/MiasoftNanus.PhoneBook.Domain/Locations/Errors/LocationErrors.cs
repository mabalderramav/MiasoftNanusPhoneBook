using MiasoftNanus.PhoneBook.Domain.Abstractions;

namespace MiasoftNanus.PhoneBook.Domain.Locations.Errors;

/// <summary>
/// Provides a set of predefined errors related to location operations.
/// This class defines errors specific to the domain of locations, consolidating
/// reusable error descriptions and codes for validation and business logic.
/// </summary>
public static class LocationErrors
{
    public static Error CodeCannotBeNullOrWhitespace =>
        new Error("LocationErrors.CodeCannotBeNullOrWhitespace", "Code cannot be null or whitespace."
        );

    public static Error DescriptionCannotBeNullOrWhitespace =>
        new Error("LocationErrors.DescriptionCannotBeNullOrWhitespace",
            "Description cannot be null or whitespace."
        );
}