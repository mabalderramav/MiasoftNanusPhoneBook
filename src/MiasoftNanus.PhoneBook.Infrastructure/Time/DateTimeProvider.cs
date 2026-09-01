using MiasoftNanus.PhoneBook.Application.Abstractions.Time;

namespace MiasoftNanus.PhoneBook.Infrastructure.Time;

/// <summary>
/// Implements the <see cref="IDateTimeProvider"/> interface to provide a standard mechanism
/// for retrieving the current UTC date and time.
/// </summary>
/// <remarks>
/// This class serves as an abstraction layer over date and time retrieval functionality,
/// ensuring consistent and testable access to the current time within the application.
/// </remarks>
internal sealed class DateTimeProvider : IDateTimeProvider
{
    public DateTime CurrentTime => DateTime.UtcNow;
}