using AuthWallet.Domain.Entities.Auth;

namespace AuthWallet.Domain.Interfaces
{
    public interface IUserRepository : IGenericRepository<User>
    {
        Task<User?> GetUserByEmail(string email);
        Task<User?> GetUserWalletByEmail(string email);
    }
}
