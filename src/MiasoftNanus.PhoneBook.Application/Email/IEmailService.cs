namespace MiasoftNanus.PhoneBook.Application.Email;

/// <summary>
/// Defines the contract for email-related operations.
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// Asynchronously sends an email with the specified recipient, subject, and body.
    /// </summary>
    /// <param name="to">The email address of the recipient.</param>
    /// <param name="subject">The subject line of the email.</param>
    /// <param name="body">The content of the email message.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public Task SendEmailAsync(string to, string subject, string body);
}