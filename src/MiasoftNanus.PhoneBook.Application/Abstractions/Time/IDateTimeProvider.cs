namespace MiasoftNanus.PhoneBook.Application.Abstractions.Time;

/// <summary>
/// Provides a mechanism to retrieve the current date and time.
/// </summary>
public interface IDateTimeProvider
{
    /// <summary>
    /// Gets the current date and time provided by the implementation of <see cref="IDateTimeProvider"/>.
    /// </summary>
    /// <remarks>
    /// This property is intended to retrieve the current timestamp in scenarios where
    /// date and time information is required, such as logging, auditing, or timestamping entities.
    /// The specific value returned is determined by the implementation of the interface.
    /// </remarks>
    DateTime CurrentTime { get; }
}