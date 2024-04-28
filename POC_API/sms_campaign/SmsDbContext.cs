using Microsoft.EntityFrameworkCore;

namespace POC_API.sms_campaign
{
    public class SmsDbContext : DbContext
    {
        public SmsDbContext(DbContextOptions<SmsDbContext> options) : base(options) { }

        public DbSet<SmsCampaigns> SmsCampaigns { get; set; }
    }
}
