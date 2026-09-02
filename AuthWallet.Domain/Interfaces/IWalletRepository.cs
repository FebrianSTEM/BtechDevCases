using AuthWallet.Domain.Entities.Wallets;

namespace AuthWallet.Domain.Interfaces
{
    public interface IWalletRepository : IGenericRepository<Wallet>
    {
        Task<Wallet?> GetWalletByUser(Guid userId);
        Task<Wallet?> GetWalletUserByWalletId(Guid walletId);
    }
}