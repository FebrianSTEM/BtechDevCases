using AuthWallet.Domain.Interfaces;
using Microsoft.EntityFrameworkCore.Storage;

namespace AuthWallet.Infrastructure.Persistence.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;

        public IUserRepository Users { get; }
        public IWalletRepository Wallets { get; }

        public UnitOfWork(AppDbContext appDbContext, 
            IUserRepository users, 
            IWalletRepository wallets)
        {
            _context = appDbContext;
            Users = users;
            Wallets = wallets;
        }

        public async Task BeginTransactionAsync() => await _context.Database.BeginTransactionAsync();

        public async Task CommitAsync() => await _context.Database.CommitTransactionAsync();

        public async Task RollbackAsync() => await _context.Database.RollbackTransactionAsync();

        public async Task<int> SaveChangesAsync() => 
            await _context.SaveChangesAsync();

        public void Dispose() => _context.Dispose();
    }
}
