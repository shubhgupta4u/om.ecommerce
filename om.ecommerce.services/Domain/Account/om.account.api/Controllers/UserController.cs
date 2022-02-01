using Microsoft.AspNetCore.Mvc;
using om.account.businesslogic.Interfaces;
using om.account.model;
using om.shared.security;
using System.Collections.Generic;
using System.Threading.Tasks;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace om.account.api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        protected readonly IUserBusinessLogic businessLogic;
        public UserController(IUserBusinessLogic businessLogic)
        {
            this.businessLogic = businessLogic;
        }
        // GET: api/<UserController>
        [Authorize]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> Get()
        {
            var all = await this.businessLogic.Get();
            return Ok(all);
        }

        // GET api/<UserController>/5
        [Authorize]
        [HttpGet("{id}")]
        public async Task<ActionResult<User>> Get(string id)
        {
            var user = await this.businessLogic.Get(id);
            return Ok(user);
        }
        // GET api/<UserController>/5
        [Authorize]
        [HttpGet("GetByEmail")]
        public async Task<ActionResult<User>> GetByEmail(string email)
        {
            var user = await this.businessLogic.GetByEmail(email);
            return Ok(user);
        }
        [Authorize]
        [HttpGet("GetByMobile")]
        public async Task<ActionResult<User>> GetByMobile(string mobile)
        {
            var user = await this.businessLogic.GetByMobile(mobile);
            return Ok(user);
        }
        // POST api/<UserController>
        [HttpPost]
        public async Task<ActionResult<User>> Post([FromBody] CreateUserRequest userRequest)
        {
            User user = await this.businessLogic.Create(userRequest);
            var actionName = nameof(Get);
            var routeValues = new { id = user.UserId };
            return CreatedAtAction(actionName, routeValues, user);
        }

        // PUT api/<UserController>/5
        [HttpPut("{id}")]
        public async Task<ActionResult> Put(string userId, [FromBody] UpdateUserRequest userRequest)
        {
            await this.businessLogic.Update(userId, userRequest);
            return Ok();
        }

        // DELETE api/<UserController>/5
        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(string id)
        {
            await this.businessLogic.Delete(id);
            return Ok();
        }
        [HttpPost("ValidateCredential")]
        public async Task<ActionResult<ValidateCredentialResponse>> ValidateCredential([FromBody] ValidateCredentialRequest request)
        {
            ValidateCredentialResponse response = await this.businessLogic.ValidateCredential(request);
            return Ok(response);
        }
    }
}
