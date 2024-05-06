using ExcelToDatabase.Data;
using Microsoft.AspNetCore.Mvc;

namespace POC_API.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class SmsMarketingRateController : Controller
    {

        private readonly ApplicationDbContext _context;

        public SmsMarketingRateController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("OverallRate")]
        public IActionResult GetOverallRate()
        {
            var totalCount = _context.SmsCampaigns.Count();
            var successCount = _context.SmsCampaigns.Count(c => c.isConverted == 1);
            var failureCount = totalCount - successCount;
            var responseCount = _context.SmsCampaigns.Count(c => c.threadId != "0");

            double successRate = 100.00;
            double failureRate = 00.00;
            
            double responseRate = Math.Round((double)responseCount / totalCount * 100, 2);
            
            double leadConversationRate = Math.Round((double)successCount / totalCount * 100, 2);

            var response = new
            {
                SuccessRate = successRate,
                FailureRate = failureRate,
                ResponseRate = responseRate,
                LeadConversationRate = leadConversationRate
            };

            return Ok(response);
        }

        [HttpGet("CampaignRate/{campaignId}")]
        public IActionResult GetCampaignRate(int campaignId)
        {
            var campaign = _context.SmsCampaigns.FirstOrDefault(c => c.campaignId == campaignId);
            if (campaign == null)
            {
                return NotFound(new { message = "Campaign not found" });
            }

            var totalCount = _context.SmsCampaigns.Count(c => c.campaignId == campaignId);
            var successCount = _context.SmsCampaigns.Count(c => c.campaignId == campaignId && c.isConverted == 1);
            var failureCount = totalCount - successCount;
            var responseCount = _context.SmsCampaigns.Count(c => c.campaignId == campaignId && c.threadId != "0" );


            double successRate = 100.00;
            double failureRate = 00.00;

            double responseRate = Math.Round((double)responseCount / totalCount * 100, 2);

            double leadConversationRate = Math.Round((double)successCount / totalCount * 100, 2);

            var response = new
            {
                SuccessRate = successRate,
                FailureRate = failureRate,
                ResponseRate = responseRate,
                LeadConversationRate = leadConversationRate
            };

            return Ok(response);
        }

    }
}
