using MiasoftNanus.PhoneBook.Application.Email;

namespace MiasoftNanus.PhoneBook.Infrastructure.Email;

/// <summary>
/// Provides functionality for sending emails using the IEmailService contract.
/// </summary>
public class EmailService : IEmailService
{
    public Task SendEmailAsync(string to, string subject, string body)
    {
        return Task.CompletedTask;
    }
}