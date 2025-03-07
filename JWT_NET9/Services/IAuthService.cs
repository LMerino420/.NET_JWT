//INTERFAZ DEL SERVICIO
using JWT_NET9.Entities;
using JWT_NET9.Models;

namespace JWT_NET9.Services
{
    public interface IAuthService
    {
        Task<User?> RegistAsync(UserDto req);
        Task<TokenResponseDto?> LoginAsync(UserDto req);
        Task<TokenResponseDto?> RefreshTokensAsync(RefreshTokenRequestDto req);
    }
}
