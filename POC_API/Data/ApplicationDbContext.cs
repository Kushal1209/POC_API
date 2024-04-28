using Microsoft.EntityFrameworkCore;
using POC_API.Models;
using POC_API.SMS;
using POC_API.sms_campaign;

namespace ExcelToDatabase.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<UserData> userDatas { get; set; }
        
        public DbSet<Registration> Registration { get; set; }
        
        public DbSet<SmsCampaigns> SmsCampaigns { get; set; }
        
        public DbSet<campaignTable> campaignTable { get; set; }


    }
}
