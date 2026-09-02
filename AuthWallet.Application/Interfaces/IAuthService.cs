using AuthWallet.Application.Dtos.Auth.Request;
using AuthWallet.Application.Dtos.Auth.Response;

namespace AuthWallet.Application.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponse> RegisterAsync(RegisterRequest request);
        Task<AuthResponse> LoginAsync(LoginRequest request);
        Task<AuthResponse> RefreshTokenAsync(RefreshTokenRequest request);
        Task LogoutAsync(Guid userId, string? refreshToken);
    }
}