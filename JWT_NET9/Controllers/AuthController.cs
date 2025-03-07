using JWT_NET9.Entities;
using JWT_NET9.Models;
using JWT_NET9.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace JWT_NET9.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController(IAuthService authSrv) : ControllerBase
    {

        //Registro de usuario
        [HttpPost("regist")]
        public async Task<ActionResult<User>> Register(UserDto req)
        {
            var user = await authSrv.RegistAsync(req);
            if (user is null)
                return BadRequest("Username already exists.!");
            return Ok(user);
        }

        //Login al sistema
        [HttpPost("login")]
        public async Task<ActionResult<TokenResponseDto>>Login(UserDto req) 
        {
            var result = await authSrv.LoginAsync(req);
            if (result is null) 
                return BadRequest("Invalid username or password.!");
            return Ok(result);
        }

        //Endpoint para refrescar token
        [HttpPost("refresh-token")]
        public async Task<ActionResult<TokenResponseDto>> RefreshToken(RefreshTokenRequestDto req)
        {
            var result = await authSrv.RefreshTokensAsync(req);
            if(result is null || result.AccessToken is null || result.RefreshToken is null)
            {
                return Unauthorized("Invalid refresh token.");
            }
            return Ok(result);
        }

        //Endpoint de autenticacion
        [Authorize]
        [HttpGet("auth-only")]
        public IActionResult AuthOnlyEndpoint()
        {
            return Ok("You are authenticated!!");
        }

        //Endpoint de autenticacion ADMIN
        [Authorize(Roles = "ADMIN")]
        [HttpGet("admin-only")]
        public IActionResult AdminOnlyEndpoint()
        {
            return Ok("You are authenticated ADMIN!!");
        }
    }
}
