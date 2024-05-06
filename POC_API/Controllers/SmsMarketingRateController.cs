using ExcelToDatabase.Data;
using Microsoft.AspNetCore.Mvc;
using POC_API.Models;

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
        public async Task<Response> GetOverallRate()
        {
            Response response = new Response();

            try
            {
                var totalCount = _context.SmsCampaigns.Count();
                var successCount = _context.SmsCampaigns.Count(c => c.isConverted == true);
                var failureCount = totalCount - successCount;
                var responseCount = _context.SmsCampaigns.Count(c => c.threadId != "0");

                double successRate = 100.00;
                double failureRate = 00.00;

                double responseRate = Math.Round((double)responseCount / totalCount * 100, 2);

                double leadConversationRate = Math.Round((double)successCount / totalCount * 100, 2);

                response.StatusCode = 200;
                response.StatusMessage = "Overall rates calculated successfully";
                response.Data = new
                {
                    SuccessRate = successRate,
                    FailureRate = failureRate,
                    ResponseRate = responseRate,
                    LeadConversationRate = leadConversationRate
                };

                return response;
            }
            catch (Exception ex)
            {
                response.StatusCode = 500;
                response.StatusMessage = $"An error occurred while calculating overall rates: {ex.Message}";
                return response;
            }
        }



        [HttpGet("CampaignRate/{campaignId}")]
        public async Task<Response> GetCampaignRate(int campaignId)
        {
            Response response = new Response();

            try
            {
                var campaign = _context.SmsCampaigns.FirstOrDefault(c => c.campaignId == campaignId);
                if (campaign == null)
                {
                    response.StatusCode = 404;
                    response.StatusMessage = "Campaign not found";
                    return response;
                }

                var totalCount = _context.SmsCampaigns.Count(c => c.campaignId == campaignId);
                var successCount = _context.SmsCampaigns.Count(c => c.campaignId == campaignId && c.isConverted == true);
                var failureCount = totalCount - successCount;
                var responseCount = _context.SmsCampaigns.Count(c => c.campaignId == campaignId && c.threadId != "0");

                double successRate = 100.00;
                double failureRate = 00.00;

                double responseRate = Math.Round((double)responseCount / totalCount * 100, 2);
                double leadConversationRate = Math.Round((double)successCount / totalCount * 100, 2);

                response.StatusCode = 200;
                response.StatusMessage = "Campaign rates calculated successfully";
                response.Data = new
                {
                    SuccessRate = successRate,
                    FailureRate = failureRate,
                    ResponseRate = responseRate,
                    LeadConversationRate = leadConversationRate
                };

                return response;
            }
            catch (Exception ex)
            {
                response.StatusCode = 500;
                response.StatusMessage = $"An error occurred while calculating campaign rates: {ex.Message}";
                return response;
            }
        }


    }
}
