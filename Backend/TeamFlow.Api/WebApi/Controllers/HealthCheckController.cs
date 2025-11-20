using Microsoft.AspNetCore.Mvc;

namespace TeamFlow.Api.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HealthCheckController : ControllerBase
    {
        /// <summary>
        /// Health check endpoint to verify the API is running
        /// </summary>
        /// <returns>Returns OK status with timestamp</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult Get()
        {
            return Ok(new
            {
                status = "healthy",
                timestamp = DateTime.UtcNow,
                version = "1.0.0"
            });
        }
    }
}
