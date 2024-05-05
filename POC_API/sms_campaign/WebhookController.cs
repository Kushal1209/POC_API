using ExcelToDatabase.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using POC_API.Models;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading;
using Telnyx;
using static POC_API.sms_campaign.WebhookController;
using static System.Net.WebRequestMethods;
using JsonException = System.Text.Json.JsonException;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace POC_API.sms_campaign
{
    [Route("api/[controller]")]
    [ApiController]
    public class WebhookController : ControllerBase
    {
        private readonly ApplicationDbContext _dbContext;

        private string api_key = "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";

        private string assistant_id;

        private UserData userdata;

        private readonly HashSet<string> _processedMessages = new HashSet<string>();

        private string[] fileIds = { "file-XqCBvbl0SLzw38vZN1Lw6uKs", "file-7mWyri4CRTRhHcvA1CefOOVH",
            "file-eLQYVVCdHhjC07PK5liRndNG", "file-d9AQR4GdlX1qEbmZb6QZwmpv", "file-Ifb9csWiVAc5jsM6S4rE10xN"
                ,"file-8Z8nyghkqdZxsMZJ7clKO4Gg" ,"file-xmXweIshBJ9KAnjp9wtaCaSc","file-WddLvLdGTOo287ju1QKrQWPl","file-Sb0g5xHXyH4oYH0O5rJp6o3J"};

        HttpClient client = new HttpClient();
        // Set up the HttpClient authorization header with your API key



        public WebhookController(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", api_key);
            assistant_id = "asst_aNt3G084cpkRLTsIMrSU3U8a";

            // Set up request headers, including the OpenAI API version
            client.DefaultRequestHeaders.Add("OpenAI-Beta", "assistants=v2");
        }

        // POST: api/webhook
        [HttpPost]
        public async Task<IActionResult> Index(string campaignId, [FromBody] Object? telnyxData)
        {
            // [FromBody] Object? telnyxData
            if (string.IsNullOrEmpty(campaignId) || telnyxData == null)
            {
                // Invalid request payload or missing campaign ID
                return BadRequest();
            }

            try
            {
                // Deserialize telnyxData to a JsonDocument
                JsonDocument jsonDocument = JsonDocument.Parse(telnyxData.ToString());

                string sent = jsonDocument.RootElement.GetProperty("data").GetProperty("event_type").GetString();

                if (sent != "message.received")
                {
                    return Ok();
                }
                // Access the "phone_number" and "text" properties from the payload
                string phoneNumber = jsonDocument.RootElement
                    .GetProperty("data")
                    .GetProperty("payload")
                    .GetProperty("from")
                    .GetProperty("phone_number")
                    .GetString().Replace("+", "");

                if (phoneNumber == null || phoneNumber.Length <= 0)
                {
                    return Ok();
                }

                string text = jsonDocument.RootElement
                    .GetProperty("data")
                    .GetProperty("payload")
                    .GetProperty("text")
                .GetString();

                string messageIdentifier = $"{phoneNumber}_{text}";

                if (_processedMessages.Contains(messageIdentifier))
                {
                    // Message already processed, skip further processing
                    return Ok();
                }

                _processedMessages.Add(messageIdentifier);

                var user = _dbContext.userDatas.Where(u => u.Number == phoneNumber).First();
                userdata = user;

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

                    string url = $"https://api.openai.com/v1/threads/{c.threadId}/messages";
                    HttpResponseMessage response1 = await client.GetAsync(url);
                    if (response1.IsSuccessStatusCode)
                    {
                        string responseContent = await response1.Content.ReadAsStringAsync();

                        JObject jsonResponse = JObject.Parse(responseContent);

                        // Extract the "data" array from the JSON response
                        JArray dataArray = (JArray)jsonResponse["data"];

                        // Initialize a list to store the message values
                        List<string> messageValues = new List<string>();

                        // Iterate over each item in the "data" array
                        foreach (JObject dataItem in dataArray)
                        {
                            JArray contentArray = (JArray)dataItem["content"];

                            // Extract the first item from the "content" array
                            JObject contentObject = (JObject)contentArray[0]; // Access the first element of the array

                            // Extract the "text" object from the first item
                            JObject textObject = (JObject)contentObject["text"];

                            // Extract the "value" string from the "text" object
                            string value = (string)textObject["value"];

                            // Add the value to the list of message values
                            messageValues.Add(value);
                        }

                        if (messageValues.Contains(text)) {
                            return Ok();
                        }

                    }



                    string apiUrl = $"https://api.openai.com/v1/threads/{c.threadId}/messages";


                    var m = new
                    {
                        role = "user",
                        content = text
                    };

                    string jsonRequestBody = JsonSerializer.Serialize(m);
                    if (client.DefaultRequestHeaders.Contains("OpenAI-Beta")) client.DefaultRequestHeaders.Remove("OpenAI-Beta");
                    client.DefaultRequestHeaders.Add("OpenAI-Beta", "assistants=v1");

                    var content = new StringContent(jsonRequestBody, Encoding.UTF8, "application/json");
                    HttpResponseMessage response = await client.PostAsync(apiUrl, content);

                    if (client.DefaultRequestHeaders.Contains("OpenAI-Beta")) client.DefaultRequestHeaders.Remove("OpenAI-Beta");
                    client.DefaultRequestHeaders.Add("OpenAI-Beta", "assistants=v2");

                    var message = await runAssistant(user.Name, assistant_id, c.threadId);
                }
                else
                {
                    //messaing dosenot exiting with ai
                    await InitializeAssiccent(inimsg.message, text, user);
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

        private async Task InitializeAssiccent(string msg1, string msg2, UserData user)
        {
            var thread = await CreateThread(msg1, msg2);
            if (thread == "")
            {
                return;
            }

            //Thread created now feed with broadcast messages
            var camp = _dbContext.SmsCampaigns.ToList();

            camp.Where(x => x.userid == user.Id).First().threadId = thread;
            await _dbContext.SaveChangesAsync();

            var message = await runAssistant(user.Name, assistant_id, thread);

            //Response recived from AI send this to as a text to the same user number



            throw new NotImplementedException();
        }

        private async Task<string> runAssistant(string name, string assistantId, string thread)
        {
            string url = $"https://api.openai.com/v1/threads/{thread}/runs";
            var requestBody = new
            {
                assistant_id = assistantId,
                instructions = "You are an AI SMS Sales chatbot for SunBridgeLeasing company. \r\n\r\nOut of many services and products of SunBridge, you will mostly focus on Equipment leasing! Unless specifically asked.\r\n\r\nYou have to reply your users with relevant information LEADING them to fill this form: \"https://www.sunbridgeleasing.com/home/LeaseProgramme\" but not too forcefully.\r\n\r\nYou can and are highly encouraged to use the website of Sunbridge Leasing whenever necessary.\r\n\r\nYou need to chat with the user for atleast 2-3 message\r\nbefore sending the form link.Unless, If they ask for something like: where can I get information and other stuff, then you can direct them to the Form link.\r\n\r\nKEEP YOUR RESPONSES SHORT Just like an SMS Response. DO NOT use bullet points. Instead use commas.\r\n\r\nYou are a ChatBot for SMS. You are a \"Salesperson\" whos purpose is to send out responses to user queries.\r\n\r\nAnswer user questions from the files attached and also visit SunBridgeLeasing's website for information to give better answers.",
                //file_ids = fileIds
            };

            string json = JsonSerializer.Serialize(requestBody);
            StringContent content = new StringContent(json, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await client.PostAsync(url, content);

            var runId = "";

            if (response.IsSuccessStatusCode)
            {
                string responseContent = await response.Content.ReadAsStringAsync();

                var runResponse = JsonSerializer.Deserialize<RunResponse>(responseContent);
                runId = runResponse?.id;
                var message = await PollRun(thread, runId);

                return message;

            }
            else
            {
                return null;
            }

            throw new NotImplementedException();
        }

        private async Task<string> PollRun(string thread, string? runId)
        {
            string url = $"https://api.openai.com/v1/threads/{thread}/runs/{runId}";
            bool isCompleted = false;

            while (!isCompleted)
            {
                HttpResponseMessage response = await client.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    string responseContent = await response.Content.ReadAsStringAsync();
                    var runStatus = JsonSerializer.Deserialize<RunStatus>(responseContent);

                    if (runStatus?.status == "completed")
                    {
                        var m = await ListThreadMessages(thread);

                        return responseContent;
                    }
                    else
                    {
                        Thread.Sleep(5000);  // Wait for 5 seconds before polling again
                    }
                }
                else
                {
                    break;
                }

            }
            throw new NotImplementedException();
        }

        private async Task<string> ListThreadMessages(string thread)
        {
            string url = $"https://api.openai.com/v1/threads/{thread}/messages";
            HttpResponseMessage response = await client.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                string responseContent = await response.Content.ReadAsStringAsync();


                var threadMessages = "";

                using (JsonDocument doc = JsonDocument.Parse(responseContent))
                {
                    JsonElement root = doc.RootElement;
                    JsonElement data = root.GetProperty("data");
                    if (data.GetArrayLength() > 0)
                    {
                        JsonElement firstMessage = data[0];
                        var role = firstMessage.GetProperty("role").GetString();
                        JsonElement firstMessageContent = firstMessage.GetProperty("content");
                        string firstMessageValue = firstMessageContent[0].GetProperty("text").GetProperty("value").GetString();
                        threadMessages = firstMessageValue;
                    }
                }
                // Extract the text value
                //string textValue = threadMessages?.data?.FirstOrDefault()?.Content?.Text?.Value;

                var messageService = new MessageService();

                var newMessage = new NewMessage
                {
                    From = "+14433923316",
                    To = "+" + userdata.Number,
                    Text = threadMessages,
                };

                await messageService.CreateAsync(newMessage);

            }

            throw new NotImplementedException();
        }

        private async Task<string> CreateThread(string message1, string message2)
        {
            var requestBody = new
            {
                messages = new[]
            {
                new { role = "assistant", content = message1 },  // msg1
                new { role = "user", content = message2 }  // msg2
            }
            };

            string json = JsonSerializer.Serialize(requestBody);
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

        public class RunResponse
        {
            public string id { get; set; }
        }

        public class RunStatus
        {
            public string status { get; set; }
        }

        public class ThreadMessages
        {
            public List<Message> data { get; set; }
        }

        public class Message
        {
            public string Role { get; set; }
            public Content Content { get; set; }
        }

        public class Content
        {
            public Text Text { get; set; }
        }

        public class Text
        {
            public string Value { get; set; }
        }

    }
}
