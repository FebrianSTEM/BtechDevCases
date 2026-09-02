using AuthWallet.Domain.Entities.Auth;

namespace AuthWallet.Domain.Interfaces
{
    public interface IRefreshTokensRepository : IGenericRepository<RefreshToken>
    {
        Task<RefreshToken?> GetStoredToken(string tokenHash);
        Task<List<RefreshToken>> GetUserActiveRefreshToken(Guid userId);
        Task<bool> AnyActiveSession(Guid userId, int inactiveTreshold);
    }
}