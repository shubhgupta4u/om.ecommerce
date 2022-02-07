using Microsoft.AspNetCore.Mvc;
using om.shared.security;

namespace om.security.api.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            return Ok("Success");
        }
        [Authorize(Roles ="admin,user")]
        [HttpGet("secure")]
        public IActionResult Secure()
        {
            string userId = this.GetLoggedInUserId<string>();
            return Ok(userId);
        }
        [Authorize(Roles = "admin")]
        [HttpGet("adminsecure")]
        public IActionResult AdminSecure()
        {
            string userId = this.GetLoggedInUserId<string>();
            return Ok(userId);
        }
        [Authorize(Roles = "user")]
        [HttpGet("usersecure")]
        public IActionResult UserSecure()
        {
            string userId = this.GetLoggedInUserId<string>();
            return Ok(userId);
        }
        [HttpGet("unsecure")]
        public IActionResult Unsecure()
        {
            return Ok("Success");
        }
    }
}
