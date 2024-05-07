using ExcelToDatabase.Data;
using Microsoft.AspNetCore.Mvc;
using POC_API.Models;
using System.Net.Http;

namespace POC_API.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class UserProfileController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpClientFactory _httpClientFactory;

        public UserProfileController(ApplicationDbContext context, IHttpClientFactory httpClientFactory)
        {
            _context = context;
            _httpClientFactory = httpClientFactory;
        }

        [HttpGet]
        public IActionResult Index()
        {
            List<UserData> userDataList = _context.userDatas.ToList();

            return Json(userDataList);
        }

        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var userData = _context.userDatas.FirstOrDefault(u => u.Id == id);

            if (userData == null)
            {
                return NotFound(new { message = "User not found" });
            }

            return Json(userData);
        }

        [HttpGet("GetMessageByUserId/{userId}")]
        public async Task<IActionResult> GetMessageByUserId(int userId)
        {
            var smsCampaign = _context.SmsCampaigns.FirstOrDefault(c => c.userid == userId);
            if (smsCampaign == null)
            {
                return NotFound(new { message = "User not found" });
            }

            if (smsCampaign.threadId == "0")
            {
                int campaignId = smsCampaign.campaignId;

                var message = _context.campaignTable.FirstOrDefault(c => c.Id == campaignId)?.message;

                if (message == null)
                {
                    return NotFound(new { message = "Message not found" });
                }

                return Json(new { Message = message });
            }
            else
            {
                var threadId = smsCampaign.threadId;
                var openAiApiUrl = $"https://api.openai.com/v1/threads/{threadId}/messages";

                try
                {
                    var client = _httpClientFactory.CreateClient();
                    var response = await client.GetAsync(openAiApiUrl);

                    if (response.IsSuccessStatusCode)
                    {
                        var responseData = await response.Content.ReadAsStringAsync();
                        return Content(responseData, "application/json");
                    }
                    else
                    {
                        return StatusCode((int)response.StatusCode, $"Failed to fetch data from OpenAI API. Status code: {response.StatusCode}");
                    }
                }
                catch (Exception ex)
                {
                    return StatusCode(500, $"An error occurred while fetching data from OpenAI API: {ex.Message}");
                }
            }

        }
    }
}
