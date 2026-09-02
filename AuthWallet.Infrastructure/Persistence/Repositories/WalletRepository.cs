using AuthWallet.Domain.Entities.Wallets;
using AuthWallet.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AuthWallet.Infrastructure.Persistence.Repositories
{
    public class WalletRepository : GenericRepository<Wallet>, IWalletRepository
    {
        public WalletRepository(AppDbContext context) : base(context)
        {

        }

        public async Task<Wallet?> GetWalletByUser(Guid userId)
        {
            return await _context.Wallets.FirstOrDefaultAsync(x => x.UserId == userId);
        }

        public async Task<Wallet?> GetWalletUserByWalletId(Guid walletId)
        {
            return await _context.Wallets.Include(x => x.User)
                .FirstOrDefaultAsync(x => x.Id == walletId);
        }
    }
}