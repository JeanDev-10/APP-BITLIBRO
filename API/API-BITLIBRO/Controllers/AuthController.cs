using System.Security.Claims;
using API_BITLIBRO.DTOs.ApiResponse;
using API_BITLIBRO.DTOs.Auth.Login;
using API_BITLIBRO.DTOs.Auth.Me;
using API_BITLIBRO.Interfaces;
using API_BITLIBRO.Services;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API_BITLIBRO.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AuthController : ControllerBase
    {

        private readonly IAuthService _authService;
        private readonly IValidator<LoginDTO> _loginValidator;
        public AuthController(IAuthService authService, IValidator<LoginDTO> loginValidator)
        {
            _authService = authService;
            _loginValidator = loginValidator;
        }
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponseData<LoginResponseDTO>>> LoginAsync([FromBody] LoginDTO loginDTO)
        {
            var validationResult = await _loginValidator.ValidateAsync(loginDTO);
            if (!validationResult.IsValid)
            {
                var errores = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
                return BadRequest(ApiResponseData<List<string>>.Fail("Errores de validación", errores));
            }
            try
            {
                LoginResponseDTO response = await _authService.LoginAsync(loginDTO);
                return Ok(ApiResponseData<LoginResponseDTO>.Success(response, "Inicio de sesión exitoso"));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ApiResponse.Fail(ex.Message));
            }
        }

        [HttpPost("logout")]
        public async Task<ActionResult<ApiResponse>> Logout()
        {
            await _authService.LogoutAsync();
            return Ok(ApiResponse.Success("Sesión cerrada correctamente"));
        }

        [HttpGet("me")]
        public async Task<ActionResult<ApiResponseData<UserInfoDTO>>> GetUserInfo()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(ApiResponse.Fail("No autorizado"));
            try
            {
                var userInfo = await _authService.GetUserInfoAsync(userId);
                return Ok(ApiResponseData<UserInfoDTO>.Success(userInfo, "Información del usuario obtenida correctamente"));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse.Fail(ex.Message));
            }
        }

    }
}
