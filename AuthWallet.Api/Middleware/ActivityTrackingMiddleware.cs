using AuthWallet.Domain.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace AuthWallet.Api.Middleware
{
    public class ActivityTrackingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IRefreshTokensRepository _refreshTokenRepository;
        private readonly IUnitOfWork _unitOfWork;

        public ActivityTrackingMiddleware(RequestDelegate next,
            IRefreshTokensRepository refreshTokensRepository,
            IUnitOfWork unitOfWork)
        {
            _next = next;
            _refreshTokenRepository = refreshTokensRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            await _next(context);

            if (context.User.Identity?.IsAuthenticated == true)
            {
                var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)
                    ?? context.User.FindFirst(JwtRegisteredClaimNames.Sub);

                if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId))
                {
                    var activeTokens = await _refreshTokenRepository.GetUserActiveRefreshToken(userId);

                    foreach (var token in activeTokens)
                    {
                        token.LastActivityAt = DateTime.UtcNow;
                    }
                    if (activeTokens.Any())
                    {
                        await _unitOfWork.SaveChangesAsync();
                    }
                }
            }
        }
    }
}
