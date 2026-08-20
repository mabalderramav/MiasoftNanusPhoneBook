using MiasoftNanus.PhoneBook.Domain.Abstractions;

namespace MiasoftNanus.PhoneBook.Domain.Users.ObjectValues;

/// <summary>
/// Represents a user-defined value object for a username within the domain.
/// Ensures the username conforms to specified creation rules and encapsulates
/// validation logic for username input.
/// </summary>
/// <remarks>
/// This class serves as a value object that is immutable once created. Creation
/// involves validation to ensure the integrity of the username. Validation
/// errors result in a failure encapsulated in a <see cref="Result{Username}"/>.
/// </remarks>
public record Username
{
    /// <summary>
    /// Gets the encapsulated value of the username. This property ensures that
    /// the username adheres to domain-specific rules defined during creation.
    /// </summary>
    /// <remarks>
    /// The value is immutable once assigned and can only be set through the creation
    /// process, which validates the input and enforces business rules. Accessing
    /// this property provides the validated username string.
    /// </remarks>
    public string Value { get; init; }

    private Username(string value)
    {
        Value = value;
    }

    /// <summary>
    /// Creates a new instance of the <see cref="Username"/> value object by validating the provided input.
    /// </summary>
    /// <param name="value">The string input to use for creating the username. Must be a non-null,
    /// non-empty, and non-whitespace value.</param>
    /// <returns>A <see cref="Result{Username}"/> representing the outcome of the creation operation.
    /// If successful, the result contains the created <see cref="Username"/> value object.
    /// Otherwise, the result contains an error describing the validation failure.</returns>
    public static Result<Username> Create(string value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? Result.Failure<Username>(Error.NullValue)
            : Result.Success(new Username(value));
    }
}