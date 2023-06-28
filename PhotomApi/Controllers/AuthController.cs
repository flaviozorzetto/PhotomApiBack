using Microsoft.AspNetCore.Mvc;
using PhotomApi.Interfaces;
using PhotomApi.Models.Dto;

namespace PhotomApi.Controllers
{
    public class AuthController : Controller
    {
        private readonly IAuthService _authService;
        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] UserLoginDto userLogin)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(error => error.Errors).Select(e => e.ErrorMessage);
                return BadRequest(errors);
            }

            var user = _authService.Authenticate(userLogin);

            if (user == null)
            {
                return NotFound("User not found");
            }

            var token = _authService.GenerateToken(user);

            return Ok(token);
        }
    }
}
