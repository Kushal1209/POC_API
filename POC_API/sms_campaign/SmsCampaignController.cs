using Microsoft.AspNetCore.Mvc;

namespace POC_API.sms_campaign
{
    [Route("api/[controller]")]
    [ApiController]
    public class SmsCampaignController : ControllerBase
    {
        private readonly SmsDbContext _dbContext;

        public SmsCampaignController(SmsDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpPost("create")]
        public IActionResult CreateSmsCampaign([FromBody] SmsCampaigns campaign)
        {
            _dbContext.SmsCampaigns.Add(campaign);
            _dbContext.SaveChanges();
            return Ok("SMS campaign created successfully.");
        }

        [HttpGet("all")]
        public IActionResult GetAllSmsCampaigns()
        {
            var campaigns = _dbContext.SmsCampaigns.ToList();
            return Ok(campaigns);
        }
    }
}
