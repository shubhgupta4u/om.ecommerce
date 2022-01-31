using Microsoft.AspNetCore.Mvc;
using om.shared.security;

namespace om.security.api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        [Authorize(Roles ="admin,user")]
        [HttpGet("secure")]
        public IActionResult Secure()
        {
            string userId = this.GetLoggedInUserId<string>();
            return Ok(User);
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
            string userId = this.GetLoggedInUserId<string>();
            return Ok(userId);
        }
    }
}
