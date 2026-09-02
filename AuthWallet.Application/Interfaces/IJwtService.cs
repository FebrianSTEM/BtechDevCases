using AuthWallet.Domain.Entities.Auth;
using System.Security.Claims;

namespace AuthWallet.Application.Interfaces
{
    public interface IJwtService
    {
        string GenerateAccessToken(User user);
        string GenerateRefreshToken();
        string HashToken(string token);
    }
}