using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Caching.Memory;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace email_check_dotnet.Controllers
{
    [ApiController]
    [Route("email")]
    public class EmailController : ControllerBase
    {
        private readonly IMemoryCache _cache;
        public EmailController(IMemoryCache cache)
        {
            _cache = cache;
        }

        [HttpPost]
        public IActionResult Post([FromBody] EmailRequest request)
        {
            var newData = new { email = request?.Email, recivedDate = DateTime.UtcNow };

            // Save using IP address to keep users isolated
            string ip = Request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            _cache.Set($"last_valid_data_{ip}", newData, TimeSpan.FromMinutes(10));

            return Ok(newData);
        }
    }

    public class EmailRequest
    {
        public string? Email { get; set; }
    }
}
