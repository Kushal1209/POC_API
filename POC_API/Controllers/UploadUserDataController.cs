using ExcelDataReader;
using ExcelToDatabase.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Logging;
using POC_API.Models;
using System.Text;

namespace POC_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UploadUserDataController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public UploadUserDataController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> UploadExcel(IFormFile file)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            if (file != null && file.Length > 0)
            {
                var uploadsFolder = $"{Directory.GetCurrentDirectory()}\\wwwroot\\Uploads\\";

                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                var filePath = Path.Combine(uploadsFolder, file.FileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                using (var stream = System.IO.File.Open(filePath, FileMode.Open, FileAccess.Read))
                {
                    using (var reader = ExcelReaderFactory.CreateReader(stream))
                    {
                        do
                        {
                            bool isHeaderSkipped = false;

                            while (reader.Read())
                            {
                                if (!isHeaderSkipped)
                                {
                                    isHeaderSkipped = true;
                                    continue;
                                }

                                UserData userData = new UserData();
                                userData.Name = reader.GetValue(0).ToString();
                                userData.Company = reader.GetValue(1).ToString();
                                userData.Email = reader.GetValue(2).ToString();
                                userData.Number = reader.GetValue(3).ToString();
                                userData.Title = reader.GetValue(4).ToString();
                                userData.IsActive = 1;

                                _context.Add(userData);
                                await _context.SaveChangesAsync();
                            }
                        } while (reader.NextResult());

                    }
                }

                // Return a success message
                return Ok("File uploaded successfully");
            }

            // Return a bad request if file is not provided
            return BadRequest("No file provided");
        }

    }

}

