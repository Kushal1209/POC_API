using System.Net.Mail;
using System.Net;

namespace POC_API.EmailSender
{
    public class EmailSender : IEmailSender
    {
        public Task SendEmailAsync(string email, string subject, string message)
        {
            var client = new SmtpClient("smtp.gmail.com", 587)
            {
                EnableSsl = true,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential("sagittarius.service@gmail.com", "gpep epjx ijlr psoy")
            };

            return client.SendMailAsync(
               new MailMessage(from: "sagittarius.service@gmail.com",
                            to: email,
                            subject: subject,
                            body: message));    
        }
    }
}
