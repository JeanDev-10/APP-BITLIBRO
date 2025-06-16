using System.Security.Claims;
using API_BITLIBRO.DTOs.Auth.Login;
using API_BITLIBRO.Services;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API_BITLIBRO.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AuthController : ControllerBase
    {

        private readonly AuthService _authService;
        private readonly IValidator<LoginDTO> _loginValidator;
        public AuthController(AuthService authService, IValidator<LoginDTO> loginValidator)
        {
            _authService = authService;
            _loginValidator = loginValidator;
        }
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<ActionResult<LoginResponseDTO>> LoginAsync([FromBody] LoginDTO loginDTO)
        {
            var validationResult = await _loginValidator.ValidateAsync(loginDTO);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors);
            }
            try
            {
                LoginResponseDTO response = await _authService.LoginAsync(loginDTO);
                return Ok(new
                {
                    message = "Inicio de sesión exitoso",
                    data = new LoginResponseDTO
                    {
                        Token = response.Token,
                        User=response.User
                    }
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            await _authService.LogoutAsync();
            return Ok(new
            {
                message = "Sesión cerrada correctamente"
            });
        }

        [HttpGet("me")]
        public async Task<IActionResult> GetUserInfo()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();
            try
            {
                var userInfo = await _authService.GetUserInfoAsync(userId);
                return Ok(new
                {
                    message = "Información del usuario obtenida correctamente",
                    data=userInfo
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

    }
}
