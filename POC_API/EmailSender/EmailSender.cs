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
                Credentials = new NetworkCredential("meet11303@gmail.com", "nmdm uxeo iuit muyk")
            };

            return client.SendMailAsync(
               new MailMessage(from: "meet11303@gmail.com",
                            to: email,
                            subject: subject,
                            body: message));    
        }
    }
}
