using ExcelToDatabase.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace POC_API.sms_campaign
{
    [Route("api/[controller]")]
    [ApiController]
    public class WebhookController : ControllerBase
    {
        private readonly ApplicationDbContext _dbContext;

        private string api_key = "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";

        private string assistant_id = "asst_aNt3G084cpkRLTsIMrSU3U8a";

        public WebhookController(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;

        }

        // POST: api/webhook
        [HttpPost]
        public async Task<IActionResult> Index(string campaignId, [FromBody] Object? telnyxData)
        {
            if (string.IsNullOrEmpty(campaignId) || telnyxData == null)
            {
                // Invalid request payload or missing campaign ID
                return BadRequest();
            }

            try
            {
                // Deserialize telnyxData to a JsonDocument
                JsonDocument jsonDocument = JsonDocument.Parse(telnyxData.ToString());

                // Access the "phone_number" and "text" properties from the payload
                string phoneNumber = jsonDocument.RootElement
                    .GetProperty("data")
                    .GetProperty("payload")
                    .GetProperty("from")
                    .GetProperty("phone_number")
                    .GetString().Replace("+", "");

                string text = jsonDocument.RootElement
                    .GetProperty("data")
                    .GetProperty("payload")
                    .GetProperty("text")
                .GetString();


                var user = _dbContext.userDatas.Where(u => u.Number == phoneNumber).First();

                if (user == null)
                {
                    return BadRequest("User not exits");
                }

                var camp = await _dbContext.SmsCampaigns.ToListAsync();

                if (camp == null || camp.Count <= 0 || !camp.FindAll(x => x.userid == user.Id).Any())
                {
                    return Ok();
                }
                var c = camp.Find(x => x.userid == user.Id);
                var inimsg = _dbContext.campaignTable.Where(x => x.Id == c.campaignId).First();
                if (inimsg == null)
                {
                    return Ok();
                }


                if (c.threadId != "0")
                {
                    //messaing exits with AI



                }
                else
                {
                    //messaing dosenot exiting with ai
                    InitializeAssiccent(inimsg.message, text, "");
                }


                return Ok("Phone number: " + phoneNumber + ", Text: " + text);
            }
            catch (JsonException ex)
            {
                // Handle JSON parsing errors
                return BadRequest("Error parsing JSON: " + ex.Message);
            }
            catch (Exception ex)
            {
                // Handle other exceptions
                return StatusCode(500, "Internal server error: " + ex.Message);
            }

            return Ok();
        }

        private async void InitializeAssiccent(string msg1, string msg2, string inst)
        {
            var thread = await CreateThread();
            if (thread == "") {
                return ;
            }

            throw new NotImplementedException();
        }

        private async Task<string> CreateThread()
        {
            HttpClient client = new HttpClient();
            // Set up the HttpClient authorization header with your API key
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", api_key);

            // Set up request headers, including the OpenAI API version
            client.DefaultRequestHeaders.Add("OpenAI-Beta", "assistants=v1");

            string json = JsonSerializer.Serialize("");
            StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await client.PostAsync("https://api.openai.com/v1/threads", content);

            if (response.IsSuccessStatusCode)
            {
                var res = await response.Content.ReadAsStringAsync();

                JsonDocument jsonDocument = JsonDocument.Parse(res);

                string threadId = jsonDocument.RootElement.GetProperty("id").GetString();

                return threadId;
            }
            else
            {
                return "0";
            }

            
        }

        public class ThreadResponse
        {
            public string Id { get; set; }
            public string Object { get; set; }
            public long CreatedAt { get; set; }
            public Dictionary<string, object> Metadata { get; set; }
        }

    }
}
