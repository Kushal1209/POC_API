using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using POC_API.Models;

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
        public Response Index(string email, string subject, string message)
        {
            Response response = new Response();

            var i = emailSender.SendEmailAsync(email, subject, message);
            
            if(i != null)
            {
                response.statusCode = 200;
                response.statusMessage = "Email send successfully";
            }
            else
            {
                response.statusCode = 400;
                response.statusMessage = "Email failed to send";
            }

            return response;
        }   
    }
}
