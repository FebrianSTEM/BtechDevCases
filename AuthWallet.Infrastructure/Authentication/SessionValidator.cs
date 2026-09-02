using AuthWallet.Application.Interfaces;
using AuthWallet.Domain.Interfaces;
using Microsoft.Extensions.Configuration;

namespace AuthWallet.Infrastructure.Authentication
{
    public class SessionValidator : ISessionValidator
    {
        private readonly IRefreshTokensRepository _refreshTokenRepository;
        private readonly IConfiguration _configuration;

        public SessionValidator(IRefreshTokensRepository refreshTokenRepository,
            IConfiguration configuration)
        {
            _refreshTokenRepository = refreshTokenRepository;
            _configuration = configuration;
        }

        public async Task<bool> HasActiveSessionAsync(Guid userId)
        {
            int inactivityMinutes = int.Parse(_configuration["Jwt:InactivityTimeoutMinutes"] ??
                throw new InvalidOperationException("Jwt:InactivityTimeoutMinutes is not configured"));

            return await _refreshTokenRepository.AnyActiveSession(userId, inactivityMinutes);
        }
    }
}
