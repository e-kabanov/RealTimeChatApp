using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RealTimeChatApp.DTOs;
using RealTimeChatApp.Models;
using RealTimeChatApp.Services;

namespace RealTimeChatApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<ActionResult<User>> Register(UserDto request)
        {
            var user = await _authService.RegisterAsync(request);

            if (user == null)
                return BadRequest("User already exists.");

            return Ok(user);
        }

        [HttpPost("login")]
        public async Task<ActionResult<string>> Login(UserDto request)
        {
            var token = await _authService.LoginAsync(request);

            if (token == null)
                return Unauthorized("Invalid username or password");

            return Ok(new { Token = token });
        }

    }
}
