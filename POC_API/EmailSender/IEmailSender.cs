namespace POC_API.EmailSender
{
    public interface IEmailSender
    {
        Task SendEmailsFromDatabaseAsync(string companyName, string subject, string message);
    }
}
