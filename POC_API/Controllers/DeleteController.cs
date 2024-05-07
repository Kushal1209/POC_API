using ExcelToDatabase.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using POC_API.Models;

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
        public async Task<Response> DeleteData()
        {
            Response response = new Response();

            try
            {
                _context.Database.ExecuteSqlRaw("DELETE FROM campaignTable");
                _context.Database.ExecuteSqlRaw("DELETE FROM SmsCampaigns");
                await _context.SaveChangesAsync();

                response.StatusCode = 200;
                response.StatusMessage = "Data deleted successfully";
                return response;
            }
            catch (Exception ex)
            {
                response.StatusCode = 500;
                response.StatusMessage = $"An error occurred while deleting data: {ex.Message}";
                return response;
            }
        }
    }
}
