using ExcelToDatabase.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace POC_API.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class DeleteController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DeleteController(ApplicationDbContext context)
        {
            _context = context;
        }



        [HttpDelete]
        public IActionResult DeleteData()
        {
            try
            {
                _context.Database.ExecuteSqlRaw("DELETE FROM campaignTable");

                _context.Database.ExecuteSqlRaw("DELETE FROM SmsCampaigns");

                _context.SaveChanges();

                return Ok("Data deleted successfully");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while deleting data: {ex.Message}");
            }
        }
    }
}
