using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace POC_API.EmailSender
{   

    [Route("api/[controller]")]
    [ApiController]
    public class EmailController : ControllerBase
    {
        private readonly IEmailSender emailSender;
        public EmailController(IEmailSender emailSender)
        {
            this.emailSender = emailSender; 
        }               


        [HttpPost]
        public async Task<IActionResult> Index(string companyName, string subject, string message)
        {
            await emailSender.SendEmailsFromDatabaseAsync(companyName, subject, message);
            return Ok("Email send successfully");
        }   
    }
}
