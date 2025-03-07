//IMPLEMENTACION DEL SERVICIO
using JWT_NET9.Data;
using JWT_NET9.Entities;
using JWT_NET9.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace JWT_NET9.Services
{
    public class AuthService(UserDbContext cntx, IConfiguration config) : IAuthService
    {
        //Valida el acceso del usuario al sistema
        public async Task<TokenResponseDto?> LoginAsync(UserDto req)
        {
            //compara los usuarios registrados en la base de datos con el que se digito por el usuario
            var user = await cntx.Users.FirstOrDefaultAsync(u => u.Username == req.Username);

            if (user is null)
            {
                return null;
            }
            if (new PasswordHasher<User>().VerifyHashedPassword(user, user.PasswordHash, req.Password) == PasswordVerificationResult.Failed)
            {
                return null;
            }

            return await CreateTokenResponse(user);

        }

        private async Task<TokenResponseDto> CreateTokenResponse(User? user)
        {
            return new TokenResponseDto
            {
                AccessToken = CreateToken(user),
                RefreshToken = await GenerateAndSaveRefreshToken(user)
            };
        }

        public async Task<TokenResponseDto?> RefreshTokensAsync(RefreshTokenRequestDto req)
        {
            var user = await ValidateRefreshTokenAsync(req.UserId,req.RefreshToken);
            if (user is null) 
            {
                return null;
            }

            return await CreateTokenResponse(user);
        }

        //Valida el token de refresco
        private async Task<User?> ValidateRefreshTokenAsync(Guid userId, string refreshToken)
        {
            var user = await cntx.Users.FindAsync(userId);
            if (user is null || user.RefreshToken != refreshToken || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
            {
                return null;
            }
            return user;
        }


        //Genera el token de refresco cuando se ha vencido el actual
        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        //Generar y guardar el token de refresco en la base de datos
        private async Task<string> GenerateAndSaveRefreshToken(User user) 
        {
            var refreshToken = GenerateRefreshToken();
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
            await cntx.SaveChangesAsync();
            return refreshToken;
        }

        //Genera un token de acceso al sistema
        private string CreateToken(User user)
        {
            //Variable para almacenar la informacion de JWT
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name,user.Username),
                new Claim(ClaimTypes.NameIdentifier,user.Id.ToString()),
                new Claim(ClaimTypes.Role,user.Rol)
            };

            //Genera el codigo de encriptacion del token
            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(config.GetValue<string>("AppSettings:Token")!));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512);

            //Descripcion del JSON que contentra el token
            var tokenDescriptor = new JwtSecurityToken(
                issuer: config.GetValue<string>("AppSettings:Issuer"),
                audience: config.GetValue<string>("AppSettings:Audience"),
                claims: claims,
                expires: DateTime.UtcNow.AddDays(1),
                signingCredentials: creds
                );

            return new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);
        }

        //Registra el usuario en la base de datos
        public async Task<User?> RegistAsync(UserDto req)
        {
            //Valida que el usuario no se encuentre registrado
            if(await cntx.Users.AnyAsync(u => u.Username == req.Username))
            {
                return null;
            }

            //Crea una instancia del usuario
            var user = new User();
            var hashedPwd = new PasswordHasher<User>()
                .HashPassword(user, req.Password);

            user.Username = req.Username;
            user.PasswordHash = hashedPwd;

            //Registra el usuario en la base de datos
            cntx.Users.Add(user);
            await cntx.SaveChangesAsync();
            
            return user;
        }
    }
}
