using AuthWallet.Domain.Entities.Auth;
using AuthWallet.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AuthWallet.Infrastructure.Persistence.Repositories
{
    public class RefreshTokenRepository : GenericRepository<RefreshToken>, IRefreshTokensRepository
    {
        public RefreshTokenRepository(AppDbContext context)  : base(context)
        {

        }

        public async Task<RefreshToken?> GetStoredToken(string tokenHash)
        {
            return await _context.RefreshTokens
                                 .Include(x => x.User)
                                 .FirstOrDefaultAsync(x => x.TokenHash == tokenHash);

        }

        public async Task<List<RefreshToken>> GetUserActiveRefreshToken(Guid userId)
        {
            return await _context.RefreshTokens.Where(x => x.User.Id == userId && !x.IsRevoked)
                .ToListAsync();
        }

        public async Task<bool> AnyActiveSession(Guid userId, int inactiveTreshold)
        {
            DateTime now = DateTime.UtcNow;
            DateTime treshold = now.AddMinutes(-inactiveTreshold);
            return await _context.RefreshTokens
                .AnyAsync(x => x.UserId == userId &&
                !x.IsRevoked &&
                x.ExpiresAt > now &&
                x.LastActivityAt >= treshold &&
                x.LastActivityAt <= now);
        }
    }
}
