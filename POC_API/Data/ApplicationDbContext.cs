using Microsoft.EntityFrameworkCore;
using POC_API.Models;

namespace ExcelToDatabase.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<UserData> userDatas { get; set; }
    }
}
