using Microsoft.AspNetCore.Mvc;
using Telnyx;

namespace POC_API.SMS
{
    /*[ApiController]
    [Route("[controller]")]
    public class SmsController : ControllerBase
    {
        private readonly string _telnyxApiKey;

        public SmsController(IConfiguration configuration)
        {
            _telnyxApiKey = configuration["Telnyx:ApiKey"];
            TelnyxConfiguration.SetApiKey(_telnyxApiKey);
        }

        [HttpPost("send-multiple")]
        public async Task<IActionResult> SendMultipleSms([FromBody] List<SmsOptions> smsRequests)
        {
            if (smsRequests == null || smsRequests.Count == 0)
            {
                return BadRequest("No SMS requests provided.");
            }

            try
            {
                foreach (var smsRequest in smsRequests)
                {
                    await SendSmsAsync(smsRequest.From, smsRequest.To, smsRequest.Text);
                }

                return Ok("SMS messages sent successfully.");
            }
            catch (TelnyxException ex)
            {
                return StatusCode(500, $"Telnyx API error: {ex.Message}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        private async Task SendSmsAsync(string from, string to, string text)
        {
            var newMessage = new NewMessage
            {
                From = from,
                To = to,
                Text = text
            };

            var messageService = new MessageService(); // Create an instance of MessageService
            await messageService.CreateAsync(newMessage);
        }



    }*/

    [Route("api/[controller]")]
    [ApiController]
    public class SmsController : ControllerBase
    {

        private readonly SmsService _smsService;

        public SmsController(SmsService smsService)
        {
            _smsService = smsService;
        }

        /*[HttpPost("send-multiple")]
        public async Task<IActionResult> SendMultipleSms([FromBody] SmsMessage[] messages)
        {
            if (messages == null || messages.Length == 0)
            {
                return BadRequest("No SMS messages provided.");
            }

            try
            {
                await _smsService.SendMultipleSmsAsync(messages);
                return Ok("Multiple SMS messages sent successfully.");
            }
            catch (TelnyxException ex)
            {
                return StatusCode(500, $"Telnyx API error: {ex.Message}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }*/


        [HttpPost("send-multiple")]
        public async Task<IActionResult> SendMultipleSms([FromBody] string message, string Campaign_name)
        {
            if (string.IsNullOrEmpty(message))
            {
                return BadRequest("No message provided.");
            }

            try
            {
                var x = await _smsService.SendMultipleSmsAsync(message, Campaign_name);

                if (x == "")
                {
                    return Ok("Multiple SMS messages sent successfully.");
                }

                return BadRequest(x);

            }
            catch (TelnyxException ex)
            {
                return StatusCode(500, $"Telnyx API error: {ex.Message}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }

    }

}
