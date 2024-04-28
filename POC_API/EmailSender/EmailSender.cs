using System.Net.Mail;
using System.Net;
using Microsoft.EntityFrameworkCore;
using ExcelToDatabase.Data;

namespace POC_API.EmailSender
{
    public class EmailSender : IEmailSender
    {
        private readonly ApplicationDbContext _dbContext;


        public EmailSender(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task SendEmailsFromDatabaseAsync(string companyName, string subject, string message)
        {
            // Fetch email IDs from the database (you need to implement this part)
            var emailList = await FetchEmailsFromDatabaseAsync();

            // Iterate through each email and send individual emails
            foreach (var email in emailList)
            {
                
                // Send the email
                await SendEmailAsync(email, subject, message);
            }
        }

        private async Task<List<string>> FetchEmailsFromDatabaseAsync()
        {
            // Query the userDatas table to retrieve email addresses
            var emailList = await _dbContext.userDatas
                                            .Select(u => u.Email)
                                            .ToListAsync();

            return emailList;
        }

        public async Task SendEmailAsync(string email, string subject, string message)
        {
            try
            {
                // Send the email using SMTP
                var client = new SmtpClient("smtp.gmail.com", 587)
                {
                    EnableSsl = true,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential("meet11303@gmail.com", "nmdm uxeo iuit muyk")
                };

                // Create a new MailMessage object
                var mailMessage = new MailMessage
                {
                    From = new MailAddress("meet11303@gmail.com"),
                    Subject = subject,
                    Body = message,
                    IsBodyHtml = true
                };

                // Add the recipient email address
                mailMessage.To.Add(email);

                // Send the email asynchronously
                await client.SendMailAsync(mailMessage);
            }
            catch (Exception ex)
            {
                // Handle exceptions, log errors, etc.
                Console.WriteLine($"Failed to send email to {email}: {ex.Message}");
                // You can choose to throw or handle the exception as needed
            }
        }
    }
}
